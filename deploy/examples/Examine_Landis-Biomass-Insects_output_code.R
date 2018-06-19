library(raster)
library(dplyr)
library(tidyr)

# Update path below for location on your directory tree...
rootdir <- "C:\\Users\\localjrfoster\\Dropbox\\LANDIS\\code\\Extension-Biomass-Insects\\deploy\\examples\\"
#rootdir <- "C:\\Users\\localjrfoster\\Dropbox\\LANDIS\\code\\Extension-Biomass-Insects-vRob\\deploy\\examples\\"
#rootdir <- "C:\\Users\\localjrfoster\\Downloads\\rob run examples\\examples\\"

today <-  paste(Sys.Date(),"b",sep="")

ilog <- read.csv(paste(rootdir,"insects-log.csv",sep=""),header=T)

ilog$InsectName <- substr(ilog$InsectName,2,nchar(levels(ilog$InsectName)[1]))
nyrs <- dim(ilog %>% filter(InsectName=="Insect1"))[1]
yearsin <- 1:nyrs
maski <- raster(paste(rootdir,"biomass\\biomass-","TotalBiomass","-",0,".bin",sep=""))

i1 <- ilog %>% filter(InsectName=="Insect1")
barplot(i1$MeanDefoliation,names.arg=yearsin-1,ylim=c(0,.5),col=rgb(0,0,1,0.5))
i2 <- ilog %>% filter(InsectName=="Insect2")
barplot(i2$MeanDefoliation,add=T,col=rgb(1,0,0,.5),ylim=c(0,.5))
i3 <- ilog %>% filter(InsectName=="Insect3")
barplot(i3$MeanDefoliation,add=T,col=rgb(0,1,0,.5),ylim=c(0,.5))
mtext("Mean Defoliation By Insect")
legend("topright",c("insect1","insect2","insect3"),fill=c(rgb(0,0,1,0.5),rgb(1,0,0,.5),rgb(0,1,0,.5)))
## BiomassRemovedInsect2  InitialPatchMapInsect3 

insects <- c("Insect1","Insect2","Insect3")
bioremovedall <- rep(0,length(yearsin))

for (k in 1:3) {
  spi <- insects[k]
  #spi <- "Insect2"
  xdim <- dim(maski)[2]
  ydim <- dim(maski)[1]

  ilog[,c(1:5,10)] %>% filter(InsectName==spi)
  imagetype <-  ".bin"##".img" ## ".bin"
  insectdir <- "outputs\\insects\\"#"" #"outputs\\insects\\"
  maxMg <- 100
  meand <- c(0)
  # Loop through maps of percent defoliation, Insect k
  for (i in 1:(length(yearsin)-1)) {
    namei <- paste(rootdir,insectdir,spi,"-",i,imagetype,sep="")
    tbi <- raster(namei)
    if (exists("cumdefo") & i>=1) {
      #cumdefolast <- raster(cumdefo,layer=(i-3))
      #cumdefonew <- tbi + cumdefolast
      if (k==1) {
      cumdefo <- stack(cumdefo,tbi)
      }
      if (k>1) {
        cumdefo[[i]] <- cumdefo[[i]] + tbi
      }
    }
    if (k==1 & i==1) {
      cumdefo <- stack(tbi)
      }

    val <- getValues(tbi) #get raster values
    m <- mean(val,na.rm=T) #remove NAs and compute mean
    meand <- c(meand,m)
    plot(tbi,col=gray((0:32)/32),xlim=c(0,xdim),zlim=c(0,maxMg),
         ylim=c(0,ydim),xlab=paste(0+i),main=paste("Defol. -",spi," (White ~ ",maxMg," %)",sep=""))
  
    rm(tbi)
  }

  plot(meand/100,ilog$MeanDefoliation[which(ilog$InsectName==spi)],
       pch=21,bg="grey",ylab="Mean Defo Log File",xlab="Mean Defo Maps")
  abline(0,1)

  spi2 <- paste("BiomassRemoved",spi,sep="")
  maxMg <- 50
  sumi <- c(0)
  for (i in 1:(length(yearsin)-1)) {
    namei <- paste(rootdir,insectdir,spi2,"-",i,imagetype,sep="")
    tbi <- raster(namei)
    val <- getValues(tbi) #get raster values
    m <- sum(val,na.rm=T) #remove NAs and compute mean
    sumi <- c(sumi,m)
    plot(tbi,col=gray((0:32)/32),xlim=c(0,xdim),zlim=c(0,maxMg),
         ylim=c(0,ydim),xlab=paste(0+i),main=paste("BioRemove. -",spi," (White ~ ",maxMg," Mg/ha)",sep=""))
  
    rm(tbi)
  }

  plot(sumi,ilog$MortalityBiomass[which(ilog$InsectName==spi)],
       pch=21,bg="grey",ylab="Mean BioM Log File",xlab="Mean BiomassRemoved Maps")
  abline(0,10^2)
  
  # Add biomass removed from each insect to running total
  bioremovedall <- bioremovedall + sumi

#Plot defo and bioremoved alternating same insect.
  maxMg2 <- 100
  for (i in 1:(length(yearsin)-1)) {
    namei <- paste(rootdir,insectdir,spi,"-",i,imagetype,sep="")
    namei2 <- paste(rootdir,insectdir,spi2,"-",i,imagetype,sep="")
    tbi <- raster(namei)
    tbi2 <- raster(namei2)
    plot(tbi,col=gray((0:32)/32),xlim=c(0,400),zlim=c(0,maxMg2),
         ylim=c(0,400),xlab=paste(0+i),main=paste("Defol. -",spi," (White ~ ",maxMg," %)",sep=""))
    plot(tbi2,col=gray((0:32)/32),xlim=c(0,400),zlim=c(0,maxMg2/10),
         ylim=c(0,400),xlab=paste(0+i),main=paste("BioRemove. -",spi," (White ~ ",maxMg/10," Mg/ha)",sep=""))
  
    rm(tbi,tbi2)
  }

}

mbioremovedall <- bioremovedall/(xdim*ydim) * 100 # Convert to same units (kg/ha/yr) from (Mg/ha/yr)

## "Plot annual maps of Total Biomass - mortality evident as declines"
meanbio <- c()
spb <- "TotalBiomass"
maxMg <- 170
for (i in 0:(length(yearsin))) {
  namei <- paste(rootdir,"biomass\\","biomass-",spb,"-",i,".bin",sep="")
  tbi <- raster(namei)
  ext <- extent(tbi)
  plot(tbi/100,col=gray((0:32)/32),ext=ext,zlim=c(0,maxMg),
       xlab=paste(0+i),main=paste("Presence -",spb," (White ~ ",maxMg," Mg/ha)",sep=""))
  val <- getValues(tbi) #get raster values
  m <- mean(val,na.rm=T) #remove NAs and compute mean
  meanbio <- c(meanbio,m)
  rm(tbi)
}
if (spb=="woody")
{assign("meanbiod",meanbio)}
if (spb == "TotalBiomass")
{assign("meanbioL",meanbio)}

## Repeat view maps for woody biomass - mortality evident as increases during/after outbreaks
cumdefo2 <- cumsum(cumdefo)
#Plot cumdefo2 and woody biomass alternating for cumulative defo from all insects.
meanbio <- c()
spb <- "woody"
maxMg <- 170
for (i in 0:(length(yearsin))) {
  namei <- paste(rootdir,"biomass\\","biomass-",spb,"-",i,".bin",sep="")
  tbi <- raster(namei)
  if (i > 0) {
  raster::plot(cumdefo2[[i+1]],xlim=c(0,400),zlim=c(0,400),
       ylim=c(0,400),xlab=paste(0+i),main=paste("CumDefo - %)",sep=""))
  }
  plot(tbi/100,col=gray((0:32)/32),xlim=c(0,400),zlim=c(0,maxMg),
       ylim=c(0,400),xlab=paste(0+i),main=paste("Presence -",spb," (White ~ ",maxMg," Mg/ha)",sep=""))
  val <- getValues(tbi) #get raster values
  m <- mean(val,na.rm=T) #remove NAs and compute mean
  meanbio <- c(meanbio,m)
  rm(tbi)
}
if (spb=="woody")
{assign("meanbiod",meanbio)}
if (spb == "TotalBiomass")
{assign("meanbioL",meanbio)}

# Plot live, dead, and total biomass as calculated from output maps.
barplot(meanbioL+meanbiod,names.arg=c(0,yearsin),xlab="year")
barplot(meanbioL,col=rgb(1,1,0,0.5),add=T)
barplot(meanbiod,col=rgb(1,0,0,0.5),add=T)
mtext("Annual Aboveground Biomass (kg/ha/yr")
legend("topright",c("Total AGB","Live AGB", "Dead AGB"),fill=c("grey",rgb(1,1,0,0.5),rgb(1,0,0,0.5)))

bioddiff <- meanbiod[2:length(meanbiod)] - meanbiod[1:(length(meanbiod)-1)]
bioLdiff <- meanbioL[2:length(meanbioL)] - meanbioL[1:(length(meanbioL)-1)]

# Create barplot to compare annual change in live AGB with annual change in dead AGB. They should close to mirror each other!
# But, the dead pool decays some in the first year...
mp <- barplot(bioLdiff,ylim=c(-3000,3000),axisnames = FALSE)
# Now place labels closer to the x axis
mtext(text = yearsin[1:length(yearsin)-1], side = 1, at = mp, line = -10)
barplot(bioddiff,col=rgb(1,0,0,0.5),add=T)
barplot(mbioremovedall[2:length(mbioremovedall)],col=rgb(0,1,0,0.5),add=T)
#barplot(c(meani[-(1:2)],0,0)*100,col=rgb(1,0,0,0.5),add=T)
mtext("Annual Change AGB (kg/ha/yr")
legend("topright",c("Live AGB", "Dead AGB","Biomass Removed"),fill=c("grey",rgb(1,0,0,0.5),rgb(0,1,0,0.5)))

barplot(i1$MeanDefoliation,names.arg=yearsin-1,ylim=c(0,.5),col=rgb(0,0,1,0.5))
barplot(i2$MeanDefoliation,add=T,col=rgb(1,0,0,.5),ylim=c(0,.5))
barplot(i3$MeanDefoliation,add=T,col=rgb(0,1,0,.5),ylim=c(0,.5))
mtext("Mean Defoliation By Insect")
legend("topright",c("insect1","insect2","insect3"),fill=c(rgb(0,0,1,0.5),rgb(1,0,0,.5),rgb(0,1,0,.5)))


