import json
import socket
import time
from typing import Optional

from pymycobot import MyPalletizer260

from .protocol import build_led_msg, build_move_msg, sync_build_move_msg
from .errors import RobotConnectionError


class MyPalletizerController:
    _JOINT_LIMITS = {
        "j1": (-160.0, 160.0),
        "j2": (0, 90.0),
        "j3": (-60, 0),
        "j4": (-360.0, 360.0),
    }

    def __init__(self, mode: str, port: Optional[str], ip: str, udp_port: int, baudrate: int = 115200):
        self.mode = mode
        self.mc: Optional[MyPalletizer260] = None
        self.sock: Optional[socket.socket] = None
        self.udp_ip = ip
        self.udp_port = udp_port

        if mode in ("real", "both"):
            try:
                self.mc = MyPalletizer260(port, baudrate)
                time.sleep(2)
                self.mc.power_on()
                print("✅ Connected to robot on port", port)
            except Exception as e:
                raise RobotConnectionError(f"Could not connect to robot on port {port}.") from e

        if mode in ("virtual", "both"):
            self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

        print("🛫 Starting in mode:", mode)

    def move_joints(self, j1, j2, j3, j4, speed=40):
        j1 = self._clamp("j1", j1)
        j2 = self._clamp("j2", j2)
        j3 = self._clamp("j3", j3)
        j4 = self._clamp("j4", j4)
        speed = self._clamp_speed(speed)

        if self.sock:
            self._send_udp(build_move_msg(j1, j2, j3, j4, speed))

        if self.mc:
            self.mc.send_angles([j1, j2, j3, j4], speed)


    def sync_move_joints(self, j1, j2, j3, j4, speed=40):
        j1 = self._clamp("j1", j1)
        j2 = self._clamp("j2", j2)
        j3 = self._clamp("j3", j3)
        j4 = self._clamp("j4", j4)
        speed = self._clamp_speed(speed)

        if self.sock:
            self._send_udp(sync_build_move_msg(j1, j2, j3, j4, speed))

        if self.mc:
            self.mc.sync_send_angles([j1, j2, j3, j4], speed)


    def set_color(self, r, g, b):
        r, g, b = self._clamp_rgb(r, g, b)

        if self.sock:
            self._send_udp(build_led_msg(r, g, b))

        if self.mc:
            self.mc.set_color(r, g, b)


    def sleep(self, seconds: float):
        time.sleep(max(0.0, float(seconds)))

    def close(self):
        if self.sock:
            try:
                self.sock.close()
            finally:
                self.sock = None

        if self.mc:
            try:
                close_fn = getattr(self.mc, "close", None)
                if callable(close_fn):
                    close_fn()
            finally:
                self.mc = None

    def _send_udp(self, payload: dict):
        data = json.dumps(payload, separators=(",", ":")).encode("utf-8")
        self.sock.sendto(data, (self.udp_ip, self.udp_port))

    def _clamp(self, joint: str, angle: float) -> float:
        lo, hi = self._JOINT_LIMITS[joint]
        a = float(angle)
        if a < lo: return lo
        if a > hi: return hi
        return a

    @staticmethod
    def _clamp_speed(speed: int) -> int:
        s = int(speed)
        if s < 1: return 1
        if s > 100: return 100
        return s

    @staticmethod
    def _clamp_rgb(r: int, g: int, b: int):
        def c(x):
            x = int(x)
            return 0 if x < 0 else 255 if x > 255 else x
        return c(r), c(g), c(b)