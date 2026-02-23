from __future__ import annotations
from dataclasses import dataclass
from typing import Literal, Optional

from .controller import MyPalletizerController
from .errors import InvalidConfigError

Mode = Literal["virtual", "real", "both"]


@dataclass(frozen=True)
class RobotConfig:
    mode: Mode
    port: Optional[str] = None          # z.B. "COM7" / "/dev/ttyUSB0"
    ip: str = "127.0.0.1"
    udp_port: int = 5005
    baudrate: int = 115200


class Robot:

    def __init__(self, config: RobotConfig):
        self.config = config
        self._validate(config)
        self._impl = MyPalletizerController(
            mode=config.mode,
            port=config.port,
            ip=config.ip,
            udp_port=config.udp_port,
            baudrate=config.baudrate,
        )

    # --------- constructors ----------
    @classmethod
    def connect(cls, port: str, *, baudrate: int = 115200) -> "Robot":
        """Real robot only."""
        return cls(RobotConfig(mode="real", port=port, baudrate=baudrate))

    @classmethod
    def sim(cls, *, ip: str = "127.0.0.1", udp_port: int = 5005) -> "Robot":
        """Unity simulation only."""
        return cls(RobotConfig(mode="virtual", ip=ip, udp_port=udp_port))

    @classmethod
    def both(cls, port: str, *, ip: str = "127.0.0.1", udp_port: int = 5005, baudrate: int = 115200) -> "Robot":
        """Real + Unity."""
        return cls(RobotConfig(mode="both", port=port, ip=ip, udp_port=udp_port, baudrate=baudrate))

    # --------- API ----------
    def move_joints(self, j1: float, j2: float, j3: float, j4: float, speed: int = 40):
        self._impl.move_joints(j1, j2, j3, j4, speed=speed)

    def set_color(self, r: int, g: int, b: int):
        self._impl.set_color(r, g, b)

    def sleep(self, seconds: float):
        self._impl.sleep(seconds)

    def close(self):
        self._impl.close()

    # Context manager support
    def __enter__(self) -> "Robot":
        return self

    def __exit__(self, exc_type, exc, tb) -> bool:
        self.close()
        return False

    # --------- Validation ----------
    @staticmethod
    def _validate(cfg: RobotConfig):
        if cfg.mode in ("real", "both") and not cfg.port:
            raise InvalidConfigError("Mode 'real' or 'both' requires a serial port (e.g., port='COM7').")
        if cfg.udp_port <= 0 or cfg.udp_port > 65535:
            raise InvalidConfigError("udp_port must be in range 1..65535.")