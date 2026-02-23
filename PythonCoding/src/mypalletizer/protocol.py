def build_move_msg(j1: float, j2: float, j3: float, j4: float, speed: int) -> dict:
    return {
        "v": 1,
        "type": "move",
        "j1": float(j1),
        "j2": float(j2),
        "j3": float(j3),
        "j4": float(j4),
        "speed": float(speed),
    }

def build_led_msg(r: int, g: int, b: int) -> dict:
    return {
        "v": 1,
        "type": "led",
        "r": int(r),
        "g": int(g),
        "b": int(b),
    }