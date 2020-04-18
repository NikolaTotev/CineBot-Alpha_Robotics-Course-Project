import socket


shouldListen = True
HEADERLENGTH = 10
serverSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
serverSocket.bind((socket.gethostname(), 4200))
serverSocket.listen(1)


while shouldListen:
    clientSocket, address = serverSocket.accept()
    print(f"Connection to {address} as been established!")

    msg = "Welcome to the server"
    msg = f'{len(msg):<{HEADERLENGTH}}' + msg

    clientSocket.send(bytes(msg, "utf-8"))





