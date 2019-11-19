import pika
import msvcrt
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

channel = commHandler.commHandler("129.161.106.25")
channel.send("amq.topic","commandMaster",json.dumps(message)) 

# credentials = pika.PlainCredentials('guest','guest')
# connection = pika.BlockingConnection(
#     pika.ConnectionParameters(host='129.161.106.25',port=5672,virtual_host='/',credentials=credentials))
# channel = connection.channel()


# queue ='masterCommand'
# exchange='master.fanout'

# channel.exchange_declare(exchange= exchange, exchange_type='fanout', durable=True)
# channel.queue_declare(queue=queue)
# send_message = json.dumps(message)
# channel.basic_publish(exchange=exchange, routing_key=queue, body=send_message)
# print(" [x] Sent "+send_message)
# connection.close()
