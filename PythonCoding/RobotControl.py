import socket
import json
import time
import sys
from pymycobot import MyPalletizer260


class RobotControl:
    def __init__(self, mode="virtual", port="COM7", ip="127.0.0.1", udp_port=5005):
        # mode kann nun "virtual", "real" oder "both" sein
        self.mode = mode.lower()
        self.mc = None
        self.sock = None

        # --- Hardware Initialisierung (für "real" und "both") ---
        if self.mode in ["real", "both"]:
            print(f"--- Verbindungsaufbau zu Hardware ({port}) ---")
            try:
                self.mc = MyPalletizer260(port, 115200)
                time.sleep(2)

                connected = False
                for i in range(5):
                    angles = self.mc.get_angles()
                    if angles and len(angles) == 4:
                        print(f"Hardware VERBUNDEN! Winkel: {angles}")
                        connected = True
                        break
                    print(f"Suche Hardware... ({i + 1}/5)")
                    time.sleep(1)

                if not connected:
                    print("FEHLER: Hardware nicht gefunden.")
                    if self.mode == "real": sys.exit()
                else:
                    self.mc.power_on()
            except Exception as e:
                print(f"Hardware Fehler: {e}")
                if self.mode == "real": sys.exit()

        # --- Simulation Initialisierung (für "virtual" und "both") ---
        if self.mode in ["virtual", "both"]:
            print(f"--- Initialisiere Simulation ({ip}:{udp_port}) ---")
            self.udp_ip = ip
            self.udp_port = udp_port
            self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    def move_joints(self, angles, speed=50):
        # 1. Befehl an echte Hardware
        if self.mode in ["real", "both"] and self.mc:
            self.mc.send_angles(angles, speed)

        # 2. Befehl an Unity Simulation
        if self.mode in ["virtual", "both"] and self.sock:
            data = {
                "j1": float(angles[0]),
                "j2": float(angles[1]),
                "j3": float(angles[2]),
                "j4": float(angles[3]),
                "speed": speed
            }
            self.sock.sendto(json.dumps(data).encode(), (self.udp_ip, self.udp_port))

    def set_led(self, r, g, b):
        # 1. Echte Hardware
        if self.mode in ["real", "both"] and self.mc:
            self.mc.set_color(r, g, b)

        # 2. Unity Simulation
        if self.mode in ["virtual", "both"] and self.sock:
            data = {
                "type": "led",  # Markierung für Unity
                "r": int(r),
                "g": int(g),
                "b": int(b)
            }
            self.sock.sendto(json.dumps(data).encode(), (self.udp_ip, self.udp_port))


# --- Test-Skript ---
# --- Test-Skript ---
if __name__ == "__main__":
    bot = RobotControl(mode="both", port="COM7")

    print("Starte synchronisierte Bewegung...")

    # Nur Koordinaten-Listen in die Liste packen!
    test_points = [
        [0, 0, 0, 0],
        [73, 85, 0, 0],
        [-73, 85, 0, 0],
        [0, 0, 0, 0]
    ]

    # Farben-Liste (optional, um sie mit den Punkten zu synchronisieren)
    colors = [(0, 255, 0), (0, 0, 255), (255, 0, 0), (255, 255, 0), (0, 255, 255), (255, 255, 255)]

    for i, p in enumerate(test_points):
        # Farbe ändern (wir nehmen die Farbe passend zum Punkt)
        r, g, b = colors[i % len(colors)]
        bot.set_led(r, g, b)

        # Bewegung ausführen
        print(f"Fahre zu Punkt {i + 1}: {p} mit Farbe RGB({r},{g},{b})")
        bot.move_joints(p, 40)

        time.sleep(4)