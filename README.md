# Server-Handling-Systems
Collection of systems and scripts that handles file server reports


Changed the system to work on a ui that allows for the changing of how data is recorded and then save to a config file the ui uses unity and is built specifically for the platform

To setup the Report file and Sending file you need to add the path to the config file directory into the stirng "storageLocation" and then compile and it will read the config file created by the ui application adn then use that to apply data

The Ui control file has Dependancies on unity a built version is not included as it depends on where the config file is located (I plan to make a universal build for linux and windows eventually)
