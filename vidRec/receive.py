import pika

connection = pika.BlockingConnection(
    pika.ConnectionParameters(host = 'localhost', port = 1234))
channel = connection.channel()

channel.queue_declare(queue='masterCommand')


def callback(ch, method, properties, body):
    print(" [x] Received %r" % body)


channel.basic_consume(
    queue='masterCommand', on_message_callback=callback, auto_ack=True)

print(' [*] Waiting for messages. To exit press CTRL+C')
channel.start_consuming()
