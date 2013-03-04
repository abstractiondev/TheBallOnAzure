The Ball (on Windows Azure) Demo (complete)
-------------------------------------------
- This project is current snapshot of The Ball development
- This project will transform from current Demo-mode into modular core (still deploying demo to begin with)

To clone, build and deploy:
NOTE! You need Git Extensions set up; Visual Studio 2012 Update 1 Git CTP does not support submodules.

On command line:
git clone git://github.com/abstractiondev/TheBallOnAzure
cd TheBallOnAzure
gitupdateproject.cmd


Build/Execution order:
TheBallOnAzureADM.sln => build all, execute AbstractionBuilder
TheBallOnAzure.sln => build all
TheBallOnAzureConfiguration.sln => Azure deployment packages (for Azure and Dev storage)

This presentation is given 5th of March 2013 on Microsoft TechDays Finland.

Videos about the platform and deploying the demo from scratch will be added to our "Demo Videos" asap:
http://abstractiondev.wordpress.com/demo-videos/


