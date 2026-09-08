import json
import time
import os
from confluent_kafka import Producer

conf = {"bootstrap.servers": "localhost:9092"}
producer = Producer(conf)
topic_name = "field-reports"

def delivery_callback(err,msg):
    if err:
        print(f"Message failed delivery: {err}")
    else:
        print(f"Message delivered to {msg.topic()} [{msg.partition()}]")

current_dir = os.path.dirname(os.path.abspath(__file__))
file_path = os.path.join(current_dir, "Data", "field_reports.json")

def produce_reports():
    print(f"Loading data from {file_path}...")

    with open (file_path, "r", encoding="utf-8") as f:
        reports = json.load(f)
    print(f"Found {len(reports)} reports. Starting to send...")

    for report in reports:
        report_data = json.dumps(report)

        producer.produce(topic_name, report_data.encode("utf-8"), callback=delivery_callback)
        producer.poll(0)

        time.sleep(0.5)

    producer.flush()
    print("Finished sending all reports!")

if __name__ == "__main__":
    produce_reports()