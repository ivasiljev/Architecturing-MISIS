#!/usr/bin/env python3
"""
Simple webhook simulator for receiving Grafana and Prometheus alerts
Usage: python webhook-simulator.py
"""

from http.server import BaseHTTPRequestHandler, HTTPServer
import json
import datetime
from urllib.parse import urlparse

class WebhookHandler(BaseHTTPRequestHandler):
    def do_POST(self):
        if self.path.startswith('/webhook/grafana-alerts'):
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            
            try:
                alert_data = json.loads(post_data.decode('utf-8'))
                self.log_grafana_alert(alert_data)
                
                # Send success response
                self.send_response(200)
                self.send_header('Content-type', 'application/json')
                self.end_headers()
                self.wfile.write(b'{"status": "received"}')
                
            except json.JSONDecodeError:
                print(f"[ERROR] Invalid JSON received: {post_data}")
                self.send_response(400)
                self.end_headers()
                
        elif self.path.startswith('/webhook/prometheus-alerts'):
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            
            try:
                alert_data = json.loads(post_data.decode('utf-8'))
                self.log_prometheus_alert(alert_data)
                
                # Send success response
                self.send_response(200)
                self.send_header('Content-type', 'application/json')
                self.end_headers()
                self.wfile.write(b'{"status": "received"}')
                
            except json.JSONDecodeError:
                print(f"[ERROR] Invalid JSON received: {post_data}")
                self.send_response(400)
                self.end_headers()
        else:
            self.send_response(404)
            self.end_headers()
    
    def log_grafana_alert(self, alert_data):
        timestamp = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        print(f"\n{'='*60}")
        print(f"🚨 GRAFANA ALERT RECEIVED - {timestamp}")
        print(f"{'='*60}")
        
        # Extract basic info
        status = alert_data.get('status', 'unknown')
        title = alert_data.get('title', 'No title')
        message = alert_data.get('message', 'No message')
        
        print(f"📊 Status: {status.upper()}")
        print(f"📝 Title: {title}")
        print(f"💬 Message: {message}")
        
        # Extract alerts
        alerts = alert_data.get('alerts', [])
        if alerts:
            print(f"\n🔔 Alerts ({len(alerts)}):")
            for i, alert in enumerate(alerts, 1):
                labels = alert.get('labels', {})
                annotations = alert.get('annotations', {})
                
                print(f"  {i}. {labels.get('alertname', 'Unknown Alert')}")
                print(f"     Severity: {labels.get('severity', 'unknown')}")
                print(f"     Instance: {labels.get('instance', 'unknown')}")
                print(f"     Job: {labels.get('job', 'unknown')}")
                
                if 'summary' in annotations:
                    print(f"     Summary: {annotations['summary']}")
                if 'description' in annotations:
                    print(f"     Description: {annotations['description']}")
                print()
        
        print(f"{'='*60}\n")

    def log_prometheus_alert(self, alert_data):
        timestamp = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        print(f"\n{'='*60}")
        print(f"⚡ PROMETHEUS ALERT RECEIVED - {timestamp}")  
        print(f"{'='*60}")
        
        # Extract basic info
        status = alert_data.get('status', 'unknown')
        receiver = alert_data.get('receiver', 'unknown')
        external_url = alert_data.get('externalURL', 'unknown')
        
        print(f"📊 Status: {status.upper()}")
        print(f"📡 Receiver: {receiver}")
        print(f"🔗 External URL: {external_url}")
        
        # Extract alerts
        alerts = alert_data.get('alerts', [])
        if alerts:
            print(f"\n🔔 Alerts ({len(alerts)}):")
            for i, alert in enumerate(alerts, 1):
                labels = alert.get('labels', {})
                annotations = alert.get('annotations', {})
                status = alert.get('status', 'unknown')
                starts_at = alert.get('startsAt', 'unknown')
                ends_at = alert.get('endsAt', 'unknown')
                
                print(f"  {i}. {labels.get('alertname', 'Unknown Alert')}")
                print(f"     Status: {status}")
                print(f"     Severity: {labels.get('severity', 'unknown')}")
                print(f"     Instance: {labels.get('instance', 'unknown')}")
                print(f"     Job: {labels.get('job', 'unknown')}")
                print(f"     Service: {labels.get('service', 'unknown')}")
                print(f"     Team: {labels.get('team', 'unknown')}")
                print(f"     Starts At: {starts_at}")
                if ends_at != '0001-01-01T00:00:00Z':
                    print(f"     Ends At: {ends_at}")
                
                if 'summary' in annotations:
                    print(f"     Summary: {annotations['summary']}")
                if 'description' in annotations:
                    print(f"     Description: {annotations['description']}")
                
                # Show fingerprint for debugging
                fingerprint = alert.get('fingerprint', 'unknown')
                print(f"     Fingerprint: {fingerprint}")
                print()
        
        # Show group info if available
        group_labels = alert_data.get('groupLabels', {})
        if group_labels:
            print(f"🏷️  Group Labels: {group_labels}")
        
        common_labels = alert_data.get('commonLabels', {})
        if common_labels:
            print(f"🏷️  Common Labels: {common_labels}")
            
        common_annotations = alert_data.get('commonAnnotations', {})
        if common_annotations:
            print(f"📝 Common Annotations: {common_annotations}")
        
        print(f"{'='*60}\n")
    
    def log_message(self, format, *args):
        # Override to reduce logging noise
        pass

def run_webhook_server(port=8090):
    server_address = ('', port)
    httpd = HTTPServer(server_address, WebhookHandler)
    
    print(f"🌐 Webhook server starting on port {port}")
    print(f"📡 Listening for Grafana alerts at: http://localhost:{port}/webhook/grafana-alerts")
    print(f"⚡ Listening for Prometheus alerts at: http://localhost:{port}/webhook/prometheus-alerts")
    print("Press Ctrl+C to stop\n")
    
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\n🛑 Shutting down webhook server...")
        httpd.server_close()

if __name__ == '__main__':
    run_webhook_server() 