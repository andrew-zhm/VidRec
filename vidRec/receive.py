import pika


queue ='masterCommand'
exchange='master.fanout'

#connect to the specific channel. 
connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='127.0.0.1',port=5672))
channel = connection.channel()

#declare the queue, and get the queue answer 
#decalre the exchange
#bind the exchange to the queue (so that it can send all the messages to all the listeners)
result = channel.queue_declare('',exclusive=True)
queue = result.method.queue
channel.exchange_declare(exchange= exchange, exchange_type='fanout', durable=True)
channel.queue_bind(exchange=exchange,queue=queue)

def callback(ch, method, properties, body):
    print(" [x] Received %r" % body)

#start to listern 
#TODO: Change the function #callback# to your own.
channel.basic_consume(
    callback, queue=queue, no_ack=True)
print(' [*] Waiting for messages. To exit press CTRL+C')
channel.start_consuming()
