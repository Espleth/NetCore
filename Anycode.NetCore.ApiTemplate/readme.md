Postgres:
```
docker run -d --name postgres -p 5432:5432 -e POSTGRES_USER="postgres" -e POSTGRES_PASSWORD="root" postgres:latest
```

Redis:
```
docker run -d --name redis-stack \
--restart unless-stopped \
-p 6379:6379 -p 8001:8001 \
-e REDIS_ARGS="--requirepass password" \
-v /home/root/mounts/redis:/data \
redis/redis-stack:latest
```

Rabbit:
```
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 \
--restart unless-stopped \
-v /home/root/mounts/rabbitmq:/var/lib/rabbitmq \
-e RABBITMQ_DEFAULT_USER=dev \
-e RABBITMQ_DEFAULT_PASS=password \
rabbitmq:management
```

Logs (configured in unified nlog.config file):<br />
By default logs are saved in the /Logs folder next to executable.<br />
Logs are also configured to be saved in Seq. Installation:<br />
Generate password hash (if needed):<br />
```
echo 'password' | docker run --rm -i datalust/seq config hash
```
Run:
```
docker run -d --name seq \
-p 5341:80 \
--restart unless-stopped \
-v /home/root/mounts/seq:/data \
-e ACCEPT_EULA=Y \
-e SEQ_FIRSTRUN_ADMINUSERNAME=admin \
-e SEQ_FIRSTRUN_ADMINPASSWORDHASH="password_hash_here" \
datalust/seq:latest
```

Git: <br />
Commit unix-style endings: ```core.autocrlf = true/input```