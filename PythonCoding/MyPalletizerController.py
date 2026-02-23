import socket
import json
import time
from typing import Optional
from pymycobot import MyPalletizer260
from IRobotControl import IRobotControl


class MyPalletizerController(IRobotControl):
    # Joint limits according to MyPalletizer 260 docs
    _JOINT_LIMITS = {
        "j1": (-162.0, 162.0),
        "j2": (-2.0, 90.0),
        "j3": (-92.0, 60.0),
        "j4": (-180.0, 180.0),
    }

    def __init__(self, mode: str, port: str, ip: str, udp_port: int):
        self.mode = mode
        self.mc: Optional[MyPalletizer260] = None
        self.sock: Optional[socket.socket] = None
        self.udp_ip = ip
        self.udp_port = udp_port

        if mode in ("real", "both"):
            self.mc = MyPalletizer260(port, 115200)
            time.sleep(2)
            self.mc.power_on()

        if mode in ("virtual", "both"):
            self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    # ---------- Public API ----------
    def move_joints(self, j1: float, j2: float, j3: float, j4: float, speed: int = 40):
        j1 = self._clamp("j1", j1)
        j2 = self._clamp("j2", j2)
        j3 = self._clamp("j3", j3)
        j4 = self._clamp("j4", j4)
        speed = self._clamp_speed(speed)

        if self.mc:
            self.mc.send_angles([j1, j2, j3, j4], speed)

        if self.sock:
            self._send_udp({
                "type": "move",   # <- Unity unterscheidet darüber
                "j1": j1, "j2": j2, "j3": j3, "j4": j4,
                "speed": float(speed)
            })

    def set_color(self, r: int, g: int, b: int):
        r, g, b = self._clamp_rgb(r, g, b)

        if self.mc:
            self.mc.set_color(r, g, b)

        if self.sock:
            self._send_udp({
                "type": "led",
                "r": r, "g": g, "b": b
            })

    def sleep(self, seconds: float):
        time.sleep(max(0.0, float(seconds)))

    def close(self):
        # UDP
        if self.sock:
            try:
                self.sock.close()
            finally:
                self.sock = None

        # Serial
        # pymycobot hat je nach Version unterschiedliche close/disconnect; wir versuchen safe.
        if self.mc:
            try:
                # optional: self.mc.power_off()
                close_fn = getattr(self.mc, "close", None)
                if callable(close_fn):
                    close_fn()
            finally:
                self.mc = None

    # ---------- Internals ----------
    def _send_udp(self, data: dict):
        payload = json.dumps(data, separators=(",", ":")).encode("utf-8")
        self.sock.sendto(payload, (self.udp_ip, self.udp_port))

    def _clamp(self, joint: str, angle: float) -> float:
        lo, hi = self._JOINT_LIMITS[joint]
        a = float(angle)
        if a < lo: return lo
        if a > hi: return hi
        return a

    def _clamp_speed(self, speed: int) -> int:
        s = int(speed)
        if s < 1: return 1
        if s > 100: return 100
        return s

    def _clamp_rgb(self, r: int, g: int, b: int):
        def c(x):
            x = int(x)
            return 0 if x < 0 else 255 if x > 255 else x
        return c(r), c(g), c(b)