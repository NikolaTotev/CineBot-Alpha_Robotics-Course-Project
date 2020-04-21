import socket
import time
import DummyMotorScript as motorScript
import datetime
import threading
import select

class ListenerThread(threading.Thread):
    def __init__(self, threadID, name, counter, serverSoccket, HEADERLENGTH):
        print(f"{str(datetime.datetime.now())}: Initializing listener. \n ThreadID: {threadID} \n Name: {name} \n Counter: {counter}")
        threading.Thread.__init__(self)
        self.threadID = threadID
        self.name = name
        self.counter = counter
        self.serverSocket = serverSoccket
        self.HEADERLENGTH = HEADERLENGTH
        print("==========\n==========")

    def run(self):
        print(f"{str(datetime.datetime.now())}: Starting listener with name {self.name}")
        Listen(self.name, self.serverSocket, self.HEADERLENGTH)
        print("Listener thread stopping.")


shouldListen = None


def Listen(threadName, serverSocket, HEADERLENGTH):
    print(f"{str(datetime.datetime.now())}: Listening on thread {threadName}! ")
    print(f"Listen flag value is: {shouldListen}")

    read_list = [serverSocket]
    while shouldListen:
        print("Outer loop test")
        readable, writable, errored = select.select(read_list, [], [])
        for so in readable:
            if so is serverSocket:
                unitySocket, address = serverSocket.accept()
                read_list.append(unitySocket)
                print(f"{str(datetime.datetime.now())}: Connection to {address} as been established!")
            else:
                counter = 3
                while counter > 0:
                    counter -= 1
                    time.sleep(3)
                    print("{str(datetime.datetime.now())}: Boop")
                    msg = f"The time is: {time.time()}"
                    msg = f'{len(msg):<{HEADERLENGTH}}' + msg
                    unitySocket.send(bytes(msg, "utf-8"))
                    msg = unitySocket.recv(1024)
                    print(msg.decode("utf-8"))
                    if msg.decode("utf-8") == "Hi Server!":
                        print("correct message!")
                        motorScript.MoveForward()


class Server:
    def __init__(self):
        self.shouldListen = True
        self.HEADERLENGTH = 10
        self.serverVersion= "0.01"
        self.HostIP = socket.gethostname()
        self.Port = 4200
        self.serverSocket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.serverSocket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

    unitySocket = None
    joystickSocket = None

    def StartServer(self):
        print(f"{str(datetime.datetime.now())}: Starting server version {self.serverVersion}\n Available On IP: {self.HostIP} Port: {self.Port}")
        # serverSocket.bind(('192.168.12.119', 4200)) #Use when deployed
        global shouldListen
        shouldListen = True

        try:
            self.serverSocket.bind((self.HostIP, self.Port))
            self.serverSocket.listen(1)
            self.serverSocket.setblocking(False)
            pass
        except socket.error as se:
            print(f"{str(datetime.datetime.now())}: An exception was encountered during server start-up: {se}")

        print(f"{str(datetime.datetime.now())}: Server version {self.serverVersion} has successfully been started!")
        self.InitListener()

    def InitListener(self):
        print(f"{str(datetime.datetime.now())}: Starting listener.")
        listenerThread = ListenerThread(1, "Primary listener", 1, self.serverSocket, self.HEADERLENGTH)
        listenerThread.start()

        print("{str(datetime.datetime.now())} Main Threa: Simulating work time 30sec")
        time.sleep(30)
        print("{str(datetime.datetime.now())} Main Thread: Stopping listener")
        self.StopServer(listenerThread)

    def StopServer(self, listenerThread):
        global shouldListen
        shouldListen = False
        listenerThread.join(10)
        print(f"{str(datetime.datetime.now())}:Stopping server version {self.serverVersion}")

    def RestartServer(self):
        print(f"{str(datetime.datetime.now())}:Restarting server version {self.serverVersion}")








