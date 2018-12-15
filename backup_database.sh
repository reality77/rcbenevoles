docker exec -t $1 pg_dumpall -c -U rcbenevoles -f /root/db_backups/`date +%Y-%m-%d"_"%H_%M_%S`.sql
