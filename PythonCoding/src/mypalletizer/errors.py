class MyPalletizerError(Exception):
    pass

class InvalidConfigError(MyPalletizerError):
    pass

class RobotConnectionError(MyPalletizerError):
    pass