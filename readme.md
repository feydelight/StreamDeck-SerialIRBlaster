# FeyDelight's StreamDeck Plugin - Serial IRBlaster

This plugin sets the foundation for working with any serial devices. 
In this example, an Arduino nano v3 was used as the receving serial device, 
which has the ability to transmit IR signals, decode IR signals, and change 
settings. Its sketch can be found [here](https://github.com/feydelight/IRDecoderAndBlaster). 

## Features
This plugin allows you to:
1. Send a single IR blast Command
1. Send Multiple IR Blast Command, with configurable delay between each
1. Disconnect from the **Serial Device** to allow it to be used by another application (for example arduino serial port needs to be released to push a new sketch to it).

## Acknowledgement
* Big thanks to BarRaider and his [suit of packages](https://github.com/BarRaider/streamdeck-tools) that helped making this plugin a breeze
* Thanks to Elgato for making a cool device and a very configurable Plugin for it