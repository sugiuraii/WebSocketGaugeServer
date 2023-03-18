# WebSocketGaugeServer - Install and setup Docker container

## Download Docker image, and create your original image with overwriting configuration. 
Recent Docker image is uploaded on Docker Hub.
And you can create new Docker image by overwriting configuration file.

To do this,
1.  Clone or download this repository.
2.  Move to `Docker.image.install` directory.
    ```
    cd Docker.image.install
    ```
3.  Edit `appsettings.json` to configure. (See [Configure.md](Configure.md)).
4.  Build and install new image.
    ```
    docker build --tag (your original local image tag) .
    ```
(You can use your original image by changing `FROM` of `Dockerfile`)
