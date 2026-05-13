import unittest
from app.services.recommendation_engine import RecommendationEngine

class RecommendationEngineTests(unittest.TestCase):
    def test_small_dataset_rule(self) -> None:
        engine = RecommendationEngine()
        result = engine.evaluate_all({}, {}, {"rowCount": 100})
        self.assertTrue(any(r.ruleId == "R-07" for r in result))

    def test_scaling_rule(self) -> None:
        engine = RecommendationEngine()
        metrics = {}
        config = {"preprocessing": {"scaling": "none"}, "model": {"name": "SVM"}}
        result = engine.evaluate_all(metrics, config, {})
        self.assertTrue(any(r.ruleId == "R-08" for r in result))

if __name__ == "__main__":
    unittest.main()
