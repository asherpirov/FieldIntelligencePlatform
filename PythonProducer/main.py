import json
import time
import os
import logging
from confluent_kafka import Producer

logging.basicConfig(level=logging.INFO, format='%(asctime)s [%(levelname)s] %(message)s')
logger = logging.getLogger(__name__)

bootstrap_servers = os.getenv("KAFKA_BOOTSTRAP_SERVERS", "localhost:9092")
conf = {"bootstrap.servers": bootstrap_servers}
producer = Producer(conf)
topic_name = "field-reports"

def delivery_callback(err,msg):
    if err:
        logger.error(f"Message failed delivery: {err}")
    else:
        logger.info(f"Message delivered to {msg.topic()} [{msg.partition()}]")

current_dir = os.path.dirname(os.path.abspath(__file__))
file_path = os.path.join(current_dir, "Data", "field_reports.json")

def produce_reports():
    print(f"Loading data from {file_path}...")

    with open (file_path, "r", encoding="utf-8") as f:
        reports = json.load(f)
    logger.info(f"Found {len(reports)} reports. Starting to send...")

    for report in reports:
        report_data = json.dumps(report)

        producer.produce(topic_name, report_data.encode("utf-8"), callback=delivery_callback)
        producer.poll(0)

        time.sleep(0.5)

    producer.flush()
    logger.info("Finished sending all reports!")

if __name__ == "__main__":
    produce_reports()