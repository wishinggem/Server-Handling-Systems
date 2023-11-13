# Server-Handling-Systems
Collection of systems and scripts that handles server reports

A application that allows for the changing of how the reports are done for example what email they are sent to and how frequently
A script to display what the config file contains without having to aopen the application
A script to create the missing config files

Setup:
download and unzip all files in compiled folder to the user home directory then add into the sudo crontab -e a job that runs every our pointing to the master controller for example "0 * * * * sudo mono /home/user/MasterReportController.exe 
then write out and save crontab 
then launch application and then enter details press apply and then the reporting system is running
/////////

If the compiled files fail to work try downloading the source code then compile yourself using mono the ui files should work due to it using unity and compiling through there builder 

/////////
I am working on a way to edit the config file without the application if you would prefer to edit settings without ui and also and setup script to create the crontab autmoatically
