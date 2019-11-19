import pika
import time
import json
import commHandler

#start python send.py
#press either 1 or 2
#repeat the steps to send multiple commands
if __name__ == '__main__':
 print("Reading form keybord")
 print('1 for start, 2 for end')
 print('press Q to quit')
 message = {}
 while True:
  ch=input()
  # press '1' to send starting command
  if ch=='1':
   print ('start')
   message = {'command':'start'}
   break
  # press '2' to send ending command
  elif ch=='2':
   print ('end')
   message = {'command':'end'}
   break
  elif ch=='q':
   print ("shutdown!")
   break
  elif ord(ch)==0x3:
   print("shutdown")
   break
  print("Reading form keybord")
  print ('press Q or ctrl+c to quit')
  #rate.sleep()

