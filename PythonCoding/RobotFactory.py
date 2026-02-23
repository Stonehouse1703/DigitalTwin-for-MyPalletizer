from IRobotControl import IRobotControl
from MyPalletizerController import MyPalletizerController

def get_robot(mode: str = "virtual") -> IRobotControl:
    """
    Factory returns an instance following IRobotControl.
    mode: "virtual" | "real" | "both"
    """
    return MyPalletizerController(
        mode=mode,
        port="COM7",
        ip="127.0.0.1",
        udp_port=5005
    )