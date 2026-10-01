#!/usr/bin/env python3
"""Cliente de prueba para Parso: envia una peticion JSON y muestra la respuesta.

Ejemplos:
  parso_client.py '{"2":"C"}'
  parso_client.py --host 127.0.0.1 --port 1994 '{"2":"STATUS"}'
  parso_client.py '{"1":{"A":{"Total":"1500"}}}'
  parso_client.py --listen 60 '{"3":{"amountInPennies":100,"clientTransactionID":"t1","testEth":false}}'
  parso_client.py --listen              # solo escucha mensajes, hasta Ctrl+C
"""

import argparse
import json
import socket
import struct
import sys
import time

MAX_FRAME = 10 * 1024 * 1024


def recv_exact(sock, n):
    buf = bytearray()
    while len(buf) < n:
        chunk = sock.recv(n - len(buf))
        if not chunk:
            raise ConnectionError("connection closed by Parso")
        buf.extend(chunk)
    return bytes(buf)


def send_frame(sock, text):
    data = text.encode("utf-8")
    sock.sendall(struct.pack(">i", len(data)) + data)


def recv_frame(sock):
    (length,) = struct.unpack(">i", recv_exact(sock, 4))
    if length < 0 or length > MAX_FRAME:
        raise ValueError("invalid frame length %d" % length)
    return recv_exact(sock, length).decode("utf-8", errors="replace")


def is_response(obj):
    return isinstance(obj, dict) and "Success" in obj and "HTTPStatus" in obj


def show(text):
    try:
        obj = json.loads(text)
    except ValueError:
        print("[message] %s" % text)
        return None
    label = "reply" if is_response(obj) else "message"
    if label == "reply" and isinstance(obj.get("DeviceResponse"), str):
        # DeviceResponse viaja como texto; si es JSON se muestra expandido
        try:
            obj["DeviceResponse"] = json.loads(obj["DeviceResponse"])
        except ValueError:
            pass
    print("[%s] %s" % (label, json.dumps(obj, indent=2, ensure_ascii=False)))
    return obj


def main():
    ap = argparse.ArgumentParser(description="Send one request to Parso and print the reply")
    ap.add_argument("request", nargs="?", help="JSON request; omit to only listen")
    ap.add_argument("--host", default="127.0.0.1")
    ap.add_argument("--port", type=int, default=1994)
    ap.add_argument("--timeout", type=float, default=10.0,
                    help="seconds to wait for the reply (default 10)")
    ap.add_argument("--listen", type=float, nargs="?", const=0, default=None, metavar="SECONDS",
                    help="keep reading frames after the reply (broadcasts such as CARDINSERTED, "
                         "late payment replies); no value means until Ctrl+C")
    ap.add_argument("--raw", action="store_true",
                    help="send the request as-is without checking that it is valid JSON")
    args = ap.parse_args()

    if args.request is None and args.listen is None:
        ap.error("give a request, --listen, or both")

    if args.request is not None and not args.raw:
        try:
            json.loads(args.request)
        except ValueError as ex:
            ap.error("request is not valid JSON: %s" % ex)

    try:
        sock = socket.create_connection((args.host, args.port), timeout=5)
    except OSError as ex:
        print("cannot connect to %s:%d: %s" % (args.host, args.port, ex), file=sys.stderr)
        return 2

    status = 0
    try:
        if args.request is not None:
            send_frame(sock, args.request)
            sent_at = time.monotonic()
            # Un mensaje que no sea una Response (p. ej. CARDINSERTED) no es la respuesta
            while True:
                left = args.timeout - (time.monotonic() - sent_at)
                if left <= 0:
                    print("no reply within %.1f s" % args.timeout, file=sys.stderr)
                    status = 3
                    break
                sock.settimeout(left)
                try:
                    text = recv_frame(sock)
                except socket.timeout:
                    print("no reply within %.1f s" % args.timeout, file=sys.stderr)
                    status = 3
                    break
                obj = show(text)
                if is_response(obj):
                    if not obj["Success"]:
                        status = 1
                    break
                # no era una Response: se trata como mensaje espontaneo
        if args.listen is not None and status in (0, 1):
            deadline = None if args.listen == 0 else time.monotonic() + args.listen
            while True:
                if deadline is None:
                    sock.settimeout(None)
                else:
                    left = deadline - time.monotonic()
                    if left <= 0:
                        break
                    sock.settimeout(left)
                try:
                    show(recv_frame(sock))
                except socket.timeout:
                    break
    except KeyboardInterrupt:
        pass
    except (OSError, ValueError) as ex:
        print("error: %s" % ex, file=sys.stderr)
        status = 2
    finally:
        sock.close()
    return status


if __name__ == "__main__":
    sys.exit(main())
