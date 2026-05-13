import os

class Settings:
    def __init__(self) -> None:
        self.log_level = os.getenv("LOG_LEVEL", "INFO")

settings = Settings()
