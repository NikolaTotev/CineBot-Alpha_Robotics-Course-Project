import socket

HEADERLENGTH = 10

clientSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
clientSocket.connect((socket.gethostname(), 4200))

while True:
    fullMsg = ''
    newMsg = True
    while True:
        msg = clientSocket.recv(16)
        if newMsg:
            print(f"New message length: {msg[:HEADERLENGTH]}")
            msgLen = int(msg[:HEADERLENGTH])
            newMsg = False

        fullMsg += msg.decode("utf-8")

        if len(fullMsg)-HEADERLENGTH == msgLen:
            print("Received full message!")
            print(fullMsg[HEADERLENGTH:])
            newMsg = True
            fullMsg = ''

