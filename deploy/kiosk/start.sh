#!/bin/bash
# LG1 kiosk session: screen, Parso (Linux Controller) and the kiosk app, without the GNOME desktop.
# APP_DIR is $HOME/lg1/entry or $HOME/lg1/exit. Closing the app (Alt+F4) ends the session and returns to the login screen.
APP_DIR=$HOME/lg1/entry

xrandr --newmode "1600x900_60.00" 118.25 1600 1696 1856 2112 900 903 908 934 -hsync +vsync
xrandr --addmode HDMI-1 1600x900_60.00
xrandr --output HDMI-1 --mode 1600x900_60.00
xset s off -dpms s noblank

openbox &

cd $HOME/Documents/TREA && ./TREA >/dev/null 2>&1 &
sleep 5

cd "$APP_DIR"
MONO_WINFORMS_XIM_STYLE=disabled mono AA.Pango.App.exe >>"$APP_DIR/kiosk-session.txt" 2>&1

pkill -x TREA
pkill -x openbox
