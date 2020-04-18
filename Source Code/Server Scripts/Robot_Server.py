import socket
import time

shouldListen = True
HEADERLENGTH = 10
serverSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
serverSocket.bind(('192.168.12.119', 4200))
serverSocket.listen(2)
print(f"{socket.gethostname()}")
while shouldListen:
    clientSocket, address = serverSocket.accept()
    print(f"Connection to {address} as been established!")

    while True:
        time.sleep(3)
        msg = f"The time is: {time.time()}"
        msg = f'{len(msg):<{HEADERLENGTH}}' + msg
        clientSocket.send(bytes(msg, "utf-8"))





