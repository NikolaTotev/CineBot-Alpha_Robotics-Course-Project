import socket
import time
import DummyMotorScript as motorScript
import datetime
import threading

class ListenerThread(threading.Thread):
    def __init__(self, threadID, name, counter):
        threading.Thread.__init__(self)
        self.threadID = threadID
        self.name = name
        self.counter = counter

    def run(self):
        print(f"{datetime.datetime.now()}: Starting listener with name {self.name}")
        Listen(self.name)
        print("Listener has stopped!")


def Listen(threadName):
    print(f"Listening on thread {threadName}! ")
    time.sleep(4)
    print("I'm done listening!")

shouldListen = True
HEADERLENGTH = 10
serverVersion= "0.01"
HostIP = socket.gethostname()
Port = 4200
serverSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
#serverSocket.bind(('192.168.12.119', 4200))
serverSocket.bind((HostIP, Port))
serverSocket.listen(1)

unitySocket = None
joystickSocket = None

print(f"{socket.gethostname()}")



"""
def StartServer():
    print(f"{str(datetime.datetime.now())}: Starting server version {serverVersion}\n Available On IP: {HostIP} Port: {Port}")
    threading.cr
"""

#def Listener():

listenerThread = ListenerThread(1, "Primary listener", 1)
listenerThread.start()

print("The main thread has finished!")
"""
    while shouldListen:
        unitySocket, address = serverSocket.accept()
        print(f"Connection to {address} as been established!")

        while True:
            time.sleep(3)
            msg = f"The time is: {time.time()}"
            msg = f'{len(msg):<{HEADERLENGTH}}' + msg
            unitySocket.send(bytes(msg, "utf-8"))
            msg = unitySocket.recv(1024)
            print(msg.decode("utf-8"))
            if msg.decode("utf-8") == "Hi Server!":
                print("correct message!")
                motorScript.MoveForward()

"""

"""
    def StopServer():
    print(f"{str(datetime.datetime.now())}:Stopping server version {serverVersion}")

def RestartServer():
    print(f"{str(datetime.datetime.now())}:Restarting server version {serverVersion}")
    
"""






