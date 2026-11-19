#!/bin/bash
# LG1 kiosk session: screen, Parso (Linux Controller) and Entry, without the GNOME desktop.
# Closing Entry (Alt+F4) ends the session and returns to the login screen.

xrandr --newmode "1600x900_60.00" 118.25 1600 1696 1856 2112 900 903 908 934 -hsync +vsync
xrandr --addmode HDMI-1 1600x900_60.00
xrandr --output HDMI-1 --mode 1600x900_60.00
xset s off -dpms s noblank

openbox &

cd /home/TREA/Documents/TREA && ./TREA >/dev/null 2>&1 &
sleep 5

cd /home/TREA/lg1/entry
MONO_WINFORMS_XIM_STYLE=disabled mono AA.Pango.App.exe >>/home/TREA/lg1/entry/kiosk-session.txt 2>&1

pkill -x TREA
pkill -x openbox
