#include <unistd.h> 
#include <stdio.h> 
#include <sys/socket.h> 
#include <stdlib.h> 
#include <netinet/in.h> 
#include <string.h> 
#define PORT 8080
#include <sys/types.h>
#include <sys/un.h>
#include <functional>
#include <iostream>

using namespace std;

#define NAME "/var/run/socket"
#define DATA "Half a league, half a league . . ."

class Server
{
private:
	int sock;
	int msgsock;
	int rval;
	struct sockaddr_un server;
	char buf[1024];

public:
	Server()
	{
		sock = socket(AF_UNIX, SOCK_STREAM, 0);
		if (sock < 0)
		{
			perror("opening stream socket");
			exit(1);
		}

		server.sun_family = AF_UNIX;
		strcpy(server.sun_path, NAME);

		if (bind(sock, (struct sockaddr*)&server, sizeof(struct sockaddr_un)))
		{
			perror("binding stream socket");
			exit(1);
		}

		printf("Socket has name %s\n", server.sun_path);
		listen(sock, 1);

		for (;;) {
			msgsock = accept(sock, 0, 0);
			if (msgsock == -1)
				perror("accept");
			else do {
				bzero(buf, sizeof(buf));
				if ((rval = read(msgsock, buf, 1024)) < 0)
					perror("reading stream message");
				else if (rval == 0)
					printf("Ending connection\n");
				else
					printf("-->%s\n", buf);

			} while (rval > 0);
			close(msgsock);

		}
		close(sock);
		unlink(NAME);


	}
};


class Client
{
	int sock;
	struct sockaddr_un server;
	char buf[1024];
public:
	Client()
	{
		sock = socket(AF_UNIX, SOCK_STREAM, 0);
		if (sock < 0) {
			perror("opening stream socket");
			exit(1);

		}
		server.sun_family = AF_UNIX;
		strcpy(server.sun_path, NAME);


		if (connect(sock, (struct sockaddr*)&server, sizeof(struct sockaddr_un)) < 0) {
			close(sock);
			perror("connecting stream socket");
			exit(1);

		}
		if (write(sock, DATA, sizeof(DATA)) < 0)
			perror("writing on stream socket");
		close(sock);
	}
};


int main(int argc, char const* argv[])
{

	if (strcmp(argv[1], "Client"))
	{
		cout << "Starting as client." << endl;
		Client newClient = Client();
	}

	if (strcmp(argv[1], "Server"))
	{
		cout << "Starting as server." << endl;
		Server newServer = Server();
	}
	return 0;

}

