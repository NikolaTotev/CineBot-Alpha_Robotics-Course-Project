import Robot_Server
import datetime


def main():
    print(f"{str(datetime.datetime.now())}: Program started")
    S = Robot_Server.Server()
    S.StartServer()


if __name__ == "__main__":
    main()

