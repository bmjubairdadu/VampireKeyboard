@echo off
"C:\Users\Administrator\AppData\Local\Programs\Inno Setup 6\ISCC.exe" "d:\Vampire Keyboard\installer.iss" > "d:\Vampire Keyboard\iscc-result.txt" 2>&1
type "d:\Vampire Keyboard\iscc-result.txt"
