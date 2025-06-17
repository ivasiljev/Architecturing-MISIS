@echo off
echo ====================================
echo Lab 2 - Redis CLI Operations Demo
echo ====================================
echo.

echo Starting Redis CLI operations...
echo Connecting to Redis container...
echo.

echo ====================================
echo 1. STRING Operations
echo ====================================
docker exec -it jewelrystore-redis redis-cli SET "student:M8O-213B-21:15" "Ivanov Ivan Ivanovich"
docker exec -it jewelrystore-redis redis-cli GET "student:M8O-213B-21:15"
docker exec -it jewelrystore-redis redis-cli EXPIRE "student:M8O-213B-21:15" 120
docker exec -it jewelrystore-redis redis-cli TTL "student:M8O-213B-21:15"
docker exec -it jewelrystore-redis redis-cli PERSIST "student:M8O-213B-21:15"
echo.

echo ====================================
echo 2. HASH Operations
echo ====================================
docker exec -it jewelrystore-redis redis-cli HSET "student:M8O-213B-21:15:info" name "Ivanov Ivan Ivanovich" age "21" email "i.ivanov@misis.edu"
docker exec -it jewelrystore-redis redis-cli HGETALL "student:M8O-213B-21:15:info"
docker exec -it jewelrystore-redis redis-cli HGET "student:M8O-213B-21:15:info" name
docker exec -it jewelrystore-redis redis-cli HSET "student:M8O-213B-21:15:info" name "Ivan Petrov"
echo.

echo ====================================
echo 3. LIST Operations
echo ====================================
docker exec -it jewelrystore-redis redis-cli LPUSH "student:M8O-213B-21:15:timetable" "Architecture and Design Patterns"
docker exec -it jewelrystore-redis redis-cli RPUSH "student:M8O-213B-21:15:timetable" "Database Systems" "Software Engineering" "Web Development"
docker exec -it jewelrystore-redis redis-cli LRANGE "student:M8O-213B-21:15:timetable" 0 -1
docker exec -it jewelrystore-redis redis-cli LRANGE "student:M8O-213B-21:15:timetable" 0 0
docker exec -it jewelrystore-redis redis-cli LPOP "student:M8O-213B-21:15:timetable"
docker exec -it jewelrystore-redis redis-cli LRANGE "student:M8O-213B-21:15:timetable" 0 -1
echo.

echo ====================================
echo 4. SET Operations
echo ====================================
docker exec -it jewelrystore-redis redis-cli SADD "student:M8O-213B-21:15:skills" "Docker" "C#" ".NET" "Redis" "SQL Server"
docker exec -it jewelrystore-redis redis-cli SMEMBERS "student:M8O-213B-21:15:skills"
docker exec -it jewelrystore-redis redis-cli SADD "student:M8O-213B-21:15:skills" "Docker"
docker exec -it jewelrystore-redis redis-cli SADD "student:M8O-213B-21:15:skills" "Kubernetes"
docker exec -it jewelrystore-redis redis-cli SREM "student:M8O-213B-21:15:skills" "Kubernetes"
docker exec -it jewelrystore-redis redis-cli SMEMBERS "student:M8O-213B-21:15:skills"
echo.

echo ====================================
echo 5. ZSET Operations
echo ====================================
docker exec -it jewelrystore-redis redis-cli ZADD "student:M8O-213B-21:15:tasks_w_priority" 100 "Complete Lab 1" 150 "Complete Lab 2" 200 "Prepare for exam"
docker exec -it jewelrystore-redis redis-cli ZRANGE "student:M8O-213B-21:15:tasks_w_priority" 0 -1 WITHSCORES
docker exec -it jewelrystore-redis redis-cli ZRANGE "student:M8O-213B-21:15:tasks_w_priority" -1 -1 WITHSCORES
docker exec -it jewelrystore-redis redis-cli ZADD "student:M8O-213B-21:15:tasks_w_priority" 0 "Review coursework"
docker exec -it jewelrystore-redis redis-cli ZINCRBY "student:M8O-213B-21:15:tasks_w_priority" 100 "Review coursework"
docker exec -it jewelrystore-redis redis-cli ZREM "student:M8O-213B-21:15:tasks_w_priority" "Review coursework"
docker exec -it jewelrystore-redis redis-cli ZRANGE "student:M8O-213B-21:15:tasks_w_priority" 0 -1 WITHSCORES
echo.

echo ====================================
echo 6. TTL Operations Demo
echo ====================================
docker exec -it jewelrystore-redis redis-cli SET "temp:demo" "This will expire"
docker exec -it jewelrystore-redis redis-cli EXPIRE "temp:demo" 30
docker exec -it jewelrystore-redis redis-cli TTL "temp:demo"
docker exec -it jewelrystore-redis redis-cli GET "temp:demo"
echo "Key will expire in 30 seconds. Check with: docker exec -it jewelrystore-redis redis-cli GET temp:demo"
echo.

echo ====================================
echo 7. Key Management
echo ====================================
docker exec -it jewelrystore-redis redis-cli KEYS "student:M8O-213B-21:15*"
echo.
echo "To delete all student keys (optional):"
echo "docker exec -it jewelrystore-redis redis-cli DEL student:M8O-213B-21:15"
echo "docker exec -it jewelrystore-redis redis-cli DEL student:M8O-213B-21:15:info"
echo "docker exec -it jewelrystore-redis redis-cli DEL student:M8O-213B-21:15:timetable"
echo "docker exec -it jewelrystore-redis redis-cli DEL student:M8O-213B-21:15:skills"
echo "docker exec -it jewelrystore-redis redis-cli DEL student:M8O-213B-21:15:tasks_w_priority"
echo.

echo ====================================
echo Lab 2 Redis CLI Demo Complete!
echo ====================================
echo.
echo "All Redis operations have been demonstrated."
echo "Check the Redis keys with: docker exec -it jewelrystore-redis redis-cli KEYS *"
echo.
pause 