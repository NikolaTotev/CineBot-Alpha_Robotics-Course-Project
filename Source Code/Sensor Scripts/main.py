import data_extraction as extractor
import atexit


def main():
    print("Starting")
    extractor.startSensor()


def onExit():
    print("Stopping")
    extractor.serializeData()


atexit.register(onExit)

if __name__ == "__main__":
    main()