# VVVF Simulator
Simulates VVVF inverter sound on a PC.

# Description
This program is for the C# console app on VisualStudio.<br>

# Term of use
You are **free** to use the code in this program.<br>
Please:<br>
- Post the URL of this GitHub page<br>

Don’t:<br>
- Release modified code without referencing this page.<br>

# Functions
## VVVF Audio Generation
This application will export simulated vvvf inverter sound in the `.wav` extension.<br>
The sampling frequency is 192kHz.<br>

## Waveform Video Generation
This application will export video as a `.avi` extension.
![2022-02-14](https://user-images.githubusercontent.com/77259842/153803020-6615bcce-22a6-4839-b919-ea114dc12d03.png)

## Voltage Vector Video Generation
This application will export video as a `.avi` extension.

## Control stat Video Generation
This application can export video of the control stat values.<br>
The file will be the`.avi` extension. <br>
![2022-02-14 (3)](https://user-images.githubusercontent.com/77259842/153803208-18692183-b1ae-4251-96dc-ccc4ce8b3c10.png)

## Realtime Audio Generation
You can generate the audio in real time and control if the sound increases or decreases in frequency as well as the rate that the frequency increases or decreases. <br>
Key Bindings<br>
```
W - Largest Change in frequency
S - Medium Change in frequency
X - Smallest Change in frequency
B - Brake Toggle between ON/OFF
N - Mascon Toggle between ON/OFF 
R - Reselect vvvf inverter sound
Enter - Exit the program
```

# Parent Project
This program was ported from RPi-Zero-VVVF
https://github.com/JOTAN-0655/RPi-Zero-VVVF

# Contributor
・Thunderfeng<br>
https://github.com/Leifengfengfeng

・Geek of the Week<br>
https://github.com/geekotw
