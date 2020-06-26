import socket
import sys
class SocketClient(object):
    """description of class"""
    def __init__(self):
        print("Starting python client")

        # Create a UDS socket
        self.sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        
        #Set server address (this is found in the Server.cs script in
        #UnixSocketTest).
        self.server_address = '/home/pi/socketTest.sock'

    def connectClient(self):
        print("Connecting Client")       
        # Connect the socket to the port where the server is listening
        print >> sys.stderr, 'connecting to %s' % self.server_address
        try:
            self.sock.connect(self.server_address)
            print("Client Connected!")
        except socket.error as msg:
            print >> sys.stderr, msg
            sys.exit(1)

    def sendData(self, motorA, motorB):
       #  try:
            # Send data
            message = "#" + str(round(motorA, 4)) + "^" + str(round(motorB, 4)) + "#"
            #print >>sys.stderr, 'sending "%s"' % message
            self.sock.sendall(message)
         
            #amount_received = 0
            #amount_expected = len(message)
    
            #while amount_received < amount_expected:
                #data = self.sock.recv(256)
                #amount_received += len(data)
                #print >>sys.stderr, 'received "%s"' % data

    def closeSocket(self):         
            print >> sys.stderr, 'Closing Socket'
            self.sock.close()

