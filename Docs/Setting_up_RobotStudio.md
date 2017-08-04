# Setting up RobotStudio

In order to use Machina with ABB robots, you will need to install RobotStudio in your system. Not only will this include the necessary libraries you need to communicate with ABB devices, but will also give you a great tool to simulate a real robotic device, test and debug your applications, and make sure you don't die trying... ;)

- Go to [RobotStudio's downloads](http://new.abb.com/products/robotics/robotstudio/downloads) and download the lastest RobotStudio + RobotWare. Install it in trial mode, it will be enough for simulation. 

- Open RobotStudio and choose New > Solution with Station and Robot Controller. Choose a location to save your station to, and the Robot model that is matching your setup. If you are only testing this out, you can choose the smaller IRB120 for example. RobotStudio will initialize a new station with this robot model, and will automatically start it in automatic mode. 
 
![](https://github.com/garciadelcastillo/Machina/blob/master/Docs/Setting_up_RobotStudio_01.png)

- At this point, the virtual controller is present in your system, and Machina should be able to detect it. Run the 'EXAMPLE_ConnectionCheck' project. If you see a long debug dump with a lot of information from this station, it means all is good!

![](https://github.com/garciadelcastillo/Machina/blob/master/Docs/Setting_up_RobotStudio_02.png)

- There are a bunch of [tutorials](http://new.abb.com/products/robotics/robotstudio/tutorials) you can check if you want to know more about how to work with RobotStudio. 




