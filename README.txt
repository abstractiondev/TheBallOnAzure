The Ball (on Windows Azure) Demo (complete)
-------------------------------------------
- This project is current snapshot of The Ball development
- This project will transform from current Demo-mode into modular core (still deploying demo to begin with)

To clone, build and deploy:
NOTE! You need Git Extensions set up; Visual Studio 2012 Update 1 Git CTP does not support submodules.

On command line to get the full source:
git clone git://github.com/abstractiondev/TheBallOnAzure
cd TheBallOnAzure
gitupdateproject.cmd

To Build & Run:

1. buildall.cmd
2. Admin VS open TheBallAzureConfiguration.sln 
=> Debug DeveloperEmulator (to get workers up and storage up)
3. updatetheballdemo_local.cmd
=> Updates the worker role binaries to accelerator blob container
4. uploadtemplates_local.cmd
=> Uploads the templates to blob storage
5. VS open TheBallOnAzure.sln
=> Debug ServiceInterface
- If default page doens't open properly, set it to:
/auth/account/website/GettingStartedWeb/default-landing-page.html
- Google login forwards on to the page
6. Running "HTML" Project on browser allows testing templates with static content
=> The content is static copy taken from blob storage and pasted to semantically 
named location equal to actual blob storage, ie. "TheBall.Demo/HelloWorldCollection/..."

Manual Process Build/Execution order:
TheBallOnAzureADM.sln => build all, execute AbstractionBuilder
TheBallOnAzure.sln => build all
TheBallOnAzureConfiguration.sln => Azure deployment packages (for Azure and Dev storage)


This presentation is given 5th of March 2013 on Microsoft TechDays Finland.

Videos about the platform and deploying the demo from scratch will be added to our "Demo Videos" asap:
http://abstractiondev.wordpress.com/demo-videos/


