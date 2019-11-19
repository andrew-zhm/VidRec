import pika

connection = pika.BlockingConnection(
<<<<<<< HEAD
    pika.ConnectionParameters(host='129.161.106.25',port=1111))
=======
    pika.ConnectionParameters(host = 'localhost', port = 1234))
>>>>>>> cc51da32f0ab95e8d7b7b73d6346e5da139a7c48
channel = connection.channel()

channel.queue_declare(queue='masterCommand')


def callback(ch, method, properties, body):
    print(" [x] Received %r" % body)


channel.basic_consume(
    queue='masterCommand', on_message_callback=callback, auto_ack=True)

print(' [*] Waiting for messages. To exit press CTRL+C')
channel.start_consuming()
