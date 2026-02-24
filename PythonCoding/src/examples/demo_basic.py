import mypalletizer

from src.mypalletizer import Robot, RobotMode


def main():
    # 1) Nur Simulation
    # robot = Robot.sim(ip="127.0.0.1", udp_port=5005)

    # 2) Nur real
    # robot = Robot.connect(port="COM7")

    # 3) Beides
    with Robot(mode=RobotMode.BOTH, port="COM7") as robot:
        robot.set_color(0, 255, 0)
        robot.move_joints(0, 0, 0, 0, speed=40)
        robot.sleep(3)

        robot.set_color(0, 0, 255)
        robot.move_joints(74, 85, 0, 0, speed=40)
        robot.sleep(5)
        print("✅ Moved to position 1")

        '''
        robot.set_color(255, 0, 255)
        robot.move_joints(-74, 85, 0, 0, speed=40)
        robot.sleep(3)

        robot.set_color(255, 0, 0)
        robot.move_joints(0, 0, 0, 0, speed=40)
        robot.sleep(3)

        robot.move_joints(160, 0, -60, 180, speed=100)
        robot.sleep(5)
        '''

        robot.sync_move_joints(-160, 0, 0, 180, speed=100)

        robot.move_joints(0, 0, 0, 0, speed=120)


if __name__ == "__main__":
    main()