from RobotFactory import get_robot
from IRobotControl import IRobotControl

def main():
    with get_robot(mode="both") as robot:  # "virtual" / "real" / "both"
        robot.set_color(0, 255, 0)
        robot.move_joints(0, 0, 0, 0, speed=40)
        robot.sleep(1)

        robot.set_color(0, 0, 255)
        robot.move_joints(160, 0, 0, 0, speed=40)
        robot.sleep(1)

        robot.move_joints(0, 0, 0, 0, speed=40)
        robot.sleep(1)

    print("Sequence finished.")

if __name__ == "__main__":
    main()