from utils.types import JobType, QAState
from algorithms.dummy import DummyKitAlgorithm, DummyQAAlgorithm
from algorithms.item_kit import Item_kit_algo
from algorithms.qa import Seeger_QA

def get_algorithm(task_id, params, dummy):
    if params.job_type == JobType.ToolKit.value:
        if dummy:
            return DummyKitAlgorithm(params, task_id)
        # TODO: return real algorithm here
    elif params.job_type == JobType.ItemKit.value:
        if dummy:
            return DummyKitAlgorithm(params, task_id)
        # TODO: return real algorithm here
    elif params.job_type == JobType.QA.value:
        if dummy:
            return DummyQAAlgorithm(params, task_id)
        return Seeger_QA(params)

    else:
        print(f'invalid job type: {params.job_type}')
