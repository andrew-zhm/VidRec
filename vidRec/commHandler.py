# -*- coding: utf-8 -*-

import pika
import threading
import os.path
import os
import sys


LOCALHOST = 'localhost'
#SERVER_IP = '129.161.106.38'
#SERVER_IP = '128.113.21.81' # studio 2
SERVER_IP = '129.161.106.33'
# QUEUES
# SpatialContext.registry
# SpatialContext.command.boss
# SpatialContext.command.worker






class commHandler:
    
    def __init__(self, ip=None):
        if ip is None:
            ip = SERVER_IP
#        credentials = pika.PlainCredentials('cel', 'c0gnitive')
#        self.parameters = pika.ConnectionParameters(ip, 5672, 'cel', credentials, heartbeat_interval=0)
        credentials = pika.PlainCredentials('sc', 'sc')
        self.parameters = pika.ConnectionParameters(ip, 5672, '/', credentials, heartbeat=60000)
        self.connection = pika.BlockingConnection(self.parameters)
        self.SendChannel = None
        self.ReceiveChannel = None
        
        
    def __del__(self):
        if self.SendChannel is not None:
            self.SendChannel.close()
        if self.ReceiveChannel is not None:
            self.ReceiveChannel.close()
        self.connection.close()
    
    
    def send(self, exchange, routing, content):
        # exchange
        if self.SendChannel is None or self.SendChannel.is_closed:
            self.SendChannel = self.connection.channel()
            if 'fanout' in exchange:
                self.SendChannel.exchange_declare(exchange=exchange, exchange_type='fanout', durable=True)
            elif 'direct' in exchange:
                self.SendChannel.exchange_declare(exchange=exchange, exchange_type='direct', durable=True)
            elif 'topic' in exchange:
                self.SendChannel.exchange_declare(exchange=exchange, exchange_type='topic', durable=True)  
            else:
                exchange = ''
        # binding
        try:
            self.SendChannel.basic_publish(exchange=exchange,
                                          routing_key=routing,
                                          body=content)
        except pika.exceptions.ConnectionClosed:
            print(' [*] The previous connection is closed.')
            self.connection = pika.BlockingConnection(self.parameters)
            self.SendChannel = self.connection.channel()
            self.send(exchange, routing, content)


    def __consumer__(self, exchange, routing, callback):
        # exchange
        if 'fanout' in exchange:
            self.ReceiveChannel.exchange_declare(exchange=exchange, exchange_type='fanout', durable=True)
        elif 'direct' in exchange:
            self.ReceiveChannel.exchange_declare(exchange=exchange, exchange_type='direct', durable=True)
        elif 'topic' in exchange:
            self.ReceiveChannel.exchange_declare(exchange=exchange, exchange_type='topic', durable=True)  
        else:
            exchange = ''
        # queue
        result = self.ReceiveChannel.queue_declare('',exclusive=True)
        queue = result.method.queue
        # binding
        if '' == exchange:
            print(' [*] The queue binds to the default exchange')
        else:
            self.ReceiveChannel.queue_bind(exchange=exchange, 
                                    queue=queue,
                                    routing_key=routing)
        print(' [*] Listening to <{}> on <{}>'.format(routing, queue) )
#        try:
        self.ReceiveChannel.basic_consume(callback, queue=queue, no_ack=True)  # for pika==0.12.0
        # self.ReceiveChannel.basic_consume(queue=queue, on_message_callback=callback, auto_ack=True)  # for pika==1.0.1
        self.ReceiveChannel.start_consuming()
#        except pika.exceptions.ConnectionClosed:
#            print(' [*] The previous connection is closed.')
#            self.connection = pika.BlockingConnection(self.parameters)
#            self.SendChannel = self.connection.channel()
#            self.ReceiveChannel = self.connection.channel()
#            self.ReceiveChannel.basic_consume(callback, queue=queue, no_ack=True)
#            self.ReceiveChannel.start_consuming()

    
    def listen(self, exchange, routing, callback):
        self.ReceiveChannel = self.connection.channel()
        consumer_thread = threading.Thread(target=self.__consumer__, args=[exchange, routing, callback])
        consumer_thread.start()
        
    
    def close(self):
        self.connection.close()
        self.connection = None


def callback(ch, method, properties, body):
    print('ch -> {}'.format(ch))
    print('method ->', method)
    print(body.decode("utf-8"))


if __name__ == '__main__':
    commToF = commHandler('129.161.106.25')
#    commToF.listen('amq.topic', 'SpatialContext.mic.username_binding', callback)
    commToF.send('amq.topic', 'SpatialContext.mic.username_binding', 'test')
#    commToF.close()




