Build image from source and push to hub.
```
docker run --rm --privileged multiarch/qemu-user-static --reset -p yes
docker buildx rm builder
docker buildx create --name builder --driver docker-container --use
docker buildx inspect --bootstrap

docker login
docker buildx build --tag sugiuraii/websocketgaugeserver --platform linux/amd64,linux/arm64,linux/arm/v7 --push .
```
ref)
* https://stackoverflow.com/questions/60080264/docker-cannot-build-multi-platform-images-with-docker-buildx

---
Build configured image
```
cd image.install.dockerhub
(Edit appsettings.json)
docker build --tag local/wsserver .
```

Run container
```
docker run --rm -p 2016:2016 local/wsserver
```
