## Docker build and local test
This section explains how to build, tag, and run the Docker image for this project on your local machine. No prior Docker experience is required.

### Prerequisites
- [Install Docker Desktop](https://www.docker.com/products/docker-desktop/) for your operating system (Windows, macOS, or Linux).
- Make sure Docker is running (check the Docker icon in your system tray/menu bar).

### 1. Build the Docker Image
Open a terminal in the project folder (where the `Dockerfile` is located) and run:

```sh
docker build -t yourimagename .
```

- `-t yourimagename` gives your image a name. You can choose any name you like (e.g., "mywebapp").
- The `.` means "build from the current directory".

### 2. Run the Docker Container Locally
Start the container and expose port 8080 so you can access the app in your browser:

```sh
docker run -p 8080:8080 yourimagename
```

- `-p 8080:8080` maps port 8080 in the container to port 8080 on your computer.
- Replace `yourimagename` with the name you used in the build step.
- The app will start and listen on port 8080.

### 3. Test in Your Browser
Open your browser and go to:

```
http://localhost:8080
```

You should see the web app running. If you see an error, check the troubleshooting section below.

### 4. Stop the Container
Press `Ctrl+C` in the terminal to stop the running container.

---

### Troubleshooting
- **Port already in use:** If you get an error about port 8080, make sure nothing else is running on that port, or use a different port (e.g., `-p 8081:8080`).
- **Docker not found:** Make sure Docker Desktop is installed and running.
- **App not loading:** Check the terminal for errors. Make sure the build completed successfully and the app is listening on the correct port.

For more help, see the [Docker documentation](https://docs.docker.com/get-started/).
