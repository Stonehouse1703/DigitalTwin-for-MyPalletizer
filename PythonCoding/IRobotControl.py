from abc import ABC, abstractmethod

class IRobotControl(ABC):
    """CENTRAL INTERFACE: Students program against this API only."""

    @abstractmethod
    def move_joints(self, j1: float, j2: float, j3: float, j4: float, speed: int = 40):
        """Move robot by joint angles (degrees)."""
        raise NotImplementedError

    @abstractmethod
    def set_color(self, r: int, g: int, b: int):
        """Set LED color (0..255)."""
        raise NotImplementedError

    @abstractmethod
    def sleep(self, seconds: float):
        """Just wait (pause) for a certain time."""
        raise NotImplementedError

    @abstractmethod
    def close(self):
        """Release resources (serial/udp)."""
        raise NotImplementedError

    # Optional nicety: context manager
    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc, tb):
        self.close()
        return False