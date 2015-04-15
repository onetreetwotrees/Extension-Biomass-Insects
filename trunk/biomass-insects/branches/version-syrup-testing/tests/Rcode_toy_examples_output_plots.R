InitialPatchValue1 <- 1
InitialPatchValue2 <- 25
InitialPatchShapeCalibrator <- 1
InitialPatchOutbreakSensitivity <- 0.001
initialOutbreakProb <- rbeta(1000,0.84,0.20)

initialAreaCalibratorRandomNum = (runif(1000) - 0.5) * InitialPatchOutbreakSensitivity / 2
hist(initialOutbreakProb*InitialPatchOutbreakSensitivity + initialAreaCalibratorRandomNum,nclass=20,col=1)
min(initialOutbreakProb*InitialPatchOutbreakSensitivity + initialAreaCalibratorRandomNum)
#Plot histogram of initial patch shape distribution
hist(rweibull(1000,InitialPatchValue1,InitialPatchValue2),nclass=50,col=1,xlab="Area (ha?)")
mean(rweibull(1000,InitialPatchValue1,InitialPatchValue2))

ilog <- read.csv("F:\\LANDIS\\examples\\Biomass Insects\\Insects\\Insect-log.csv")

i1 <- ilog[ilog$InsectName=="Insect1",]
i2 <- ilog[ilog$InsectName=="Insect2",]
barplot(i1$MeanDefoliation,names.arg=i1$Time,cex.names=0.8,col=rgb(1,0,0,0.3),
        ylab="Mean Defoliation (%)")
barplot(i2$MeanDefoliation,names.arg=i2$Time,add=T,col=rgb(0,0,1,0.3))
legend("topleft",c("i1","i2"),fill=c(rgb(1,0,0,0.3),rgb(0,0,1,0.3)),bty="n")

ist1 <- read.table("F:\\LANDIS\\examples\\Biomass Insects\\Insects\\Insect1-Stats.txt",header=T)
ist2 <- read.table("F:\\LANDIS\\examples\\Biomass Insects\\Insects\\Insect2-Stats.txt",header=T)
biore1 <- read.table("F:\\LANDIS\\examples\\Biomass Insects\\Insects\\BiomassRemovedInsect1-Stats.txt",header=T)
biore2 <- read.table("F:\\LANDIS\\examples\\Biomass Insects\\Insects\\BiomassRemovedInsect2-Stats.txt",header=T)


barplot(ist1$Mean/100,names.arg=ist1$Time,add=T,col=rgb(1,0,0,0.3))
barplot(ist2$Mean/100,names.arg=ist2$Time,add=T,col=rgb(0,0,1,0.3))

mortMg <- (i1$MortalityBiomass + i2$MortalityBiomass)/1000
mortMgha <- mortMg/(99*99*30*30/10000)
par(new=T)
barplot(mortMgha,names.arg=i1$Time,col=rgb(1,1,0,0.3),yaxt="n")
axis(side=4)

barplot(c(0,biore2$Mean+biore1$Mean),names.arg=c(0,biore2$Time),col=1,yaxt="n",add=T,density=10,angle=45)


## Test  logic for setting ActiveOutbreak and OutbreakStartYear/StopYear
MeanTimeBetweenOutbreaks <- 4
StdDevTimeBetweenOutbreaks <- 1
MeanDuration <- 2

timeBetweenOutbreaks = MeanTimeBetweenOutbreaks + (StdDevTimeBetweenOutbreaks * runif(100))
duration = round(rexp(100,MeanDuration) + 1)
timeAfterDuration = round(timeBetweenOutbreaks) - duration
hist(timeAfterDuration,nclass=20,col=1)
duration
ActiveOutbreak = "false"

hist(round(rexp(1000,MeanDuration) + 1),nclass=100,col=1)
max(1,round(runif(1)*timeBetweenOutbreaks))

#CurrentTime = 1
if (CurrentTime == 1) {
  randomNum1 = runif(1)
  OutbreakStartYear = max(2,round((randomNum1 * timeBetweenOutbreaks + 1))) # New, try making 1st start year more random. 1st outbreak has to occur > year1 to for InitializeDefoliationPatches to work properly.
  OutbreakStopYear  = OutbreakStartYear + round(duration) - 1
}
OutbreakStartYear
OutbreakStopYear

# First Year of multiyear outbreak, don't set new outbreak timing until final year.
if (OutbreakStartYear <= CurrentTime && OutbreakStopYear > CurrentTime) {
  ActiveOutbreak = "true"
  MortalityYear <- CurrentTime + 1
  LastStartYear <- OutbreakStartYear
  LastStopYear <- OutbreakStopYear
  CurrentTime = CurrentTime + 1
  MortalityYear
  OutbreakStartYear
  OutbreakStopYear
}


## Single Outbreak year special case
if (OutbreakStartYear == CurrentTime && OutbreakStopYear == CurrentTime) {
  ActiveOutbreak = "true"
  SingleOutbreakYear = "true"
  MortalityYear <- CurrentTime + 1
  LastStartYear <- OutbreakStartYear
  LastStopYear <- OutbreakStopYear  
  OutbreakStartYear = CurrentTime + round(timeBetweenOutbreaks)
  OutbreakStopYear = OutbreakStartYear + round(duration - 1)
  CurrentTime = CurrentTime + 1
  MortalityYear
  LastStartYear
  LastStopYear
  OutbreakStartYear
  OutbreakStopYear
  
}

## Final Year of multiyear outbreak
if (OutbreakStopYear <= CurrentTime
    && timeAfterDuration > CurrentTime - OutbreakStopYear) {
  ActiveOutbreak = "true"
  MortalityYear <- CurrentTime + 1
  LastStartYear <- OutbreakStartYear
  LastStopYear <- OutbreakStopYear  
  OutbreakStartYear = CurrentTime + round(timeBetweenOutbreaks)
  OutbreakStopYear = OutbreakStartYear + round(duration - 1)
  CurrentTime = CurrentTime + 1
  MortalityYear
  LastStartYear
  LastStopYear
  OutbreakStartYear
  OutbreakStopYear
}
ActiveOutbreak

if (OutbreakStartYear > CurrentTime)
    
    (CurrentTime = CurrentTime + 1)
CurrentTime
