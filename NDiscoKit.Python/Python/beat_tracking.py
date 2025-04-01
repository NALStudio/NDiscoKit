# Python >=3.13

# pyright: basic

from collections.abc import Buffer
from dataclasses import dataclass
from typing import Any, Final, Iterable, Optional
import madmom # pyright: ignore [reportMissingTypeStubs]
import numpy

class Processor:
    def __init__(self, in_processor: madmom.processors.Processor, *out_processors: madmom.processors.Processor) -> None:
        self.in_processor: Final[madmom.processors.Processor] = in_processor
        self.out_processors: Final[tuple[madmom.processors.Processor, ...]] = out_processors
        self.results: Final[list[Buffer | None]] = [None for _ in out_processors]

def create_processor(fps: int, **kwargs) -> Processor:
    args: dict[str, Any] = {
        **_create_rnn_beat_args(fps, **kwargs),
        **_create_rnn_tempo_args(fps, **kwargs)
    }

    in_processor = madmom.features.RNNBeatProcessor(**args)

    op1 = madmom.features.DBNBeatTrackingProcessor(**args)
    op2 = madmom.features.TempoEstimationProcessor(**args)

    return Processor(in_processor, op1, op2)

#region Scrapped
# def create_beat_tracker(fps: int, **kwargs) -> Processor:
#     args = _create_rnn_beat_args(fps, **kwargs)
#
#     # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/DBNBeatTracker#L86
#     in_processor = madmom.features.RNNBeatProcessor(**args)
#     out_processor = madmom.features.DBNBeatTrackingProcessor(**args)
#
#     return Processor(in_processor, out_processor)
#
# def create_tcn_tempo_tracker(fps: int, **kwargs) -> Processor:
#     args = _create_args(
#         # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TCNTempoDetector#L65
#         fps=fps,
#         tasks = (1, ),
#         interpolate = True,
#         method = None,
#         act_smooth = None,
#
#         # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TCNTempoDetector#L60
#         hist_smooth=15,
#
#         **kwargs
#     )
#
#     args["histogram_processor"] = madmom.features.tempo.TCNTempoHistogramProcessor(**args)
#
#     # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TCNTempoDetector#L75
#     in_processor = madmom.features.beats.TCNBeatProcessor(**args)
#     out_processor = madmom.features.TempoEstimationProcessor(**args)
#
#     return Processor(in_processor, out_processor)
#
# def create_tempo_tracker(fps: int, **kwargs) -> Processor:
#     args = _create_rnn_tempo_args(fps, **kwargs)
#
#     # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TempoDetector#L126
#     in_processor = madmom.features.RNNBeatProcessor(**args)
#     out_processor = madmom.features.TempoEstimationProcessor(**args)
#
#     return Processor(in_processor, out_processor)
#
# def create_onset_tracker(fps: int, **kwargs) -> Processor:
#     args = _create_args(
#         # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/OnsetDetectorLL#L86
#         fps = fps,
#         pre_max = 1. / fps,
#         post_max = 0,
#         post_avg = 0,
#         online = True,
#
#         # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/OnsetDetectorLL#L81
#         threshold = 0.23,
#         combine = 0.03,
#         delay = 0.0,
#
#         **kwargs
#     )
#
#     # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/OnsetDetectorLL#L97
#     in_processor = madmom.features.RNNOnsetProcessor(**args)
#     out_processor = madmom.features.OnsetPeakPickingProcessor(**args)
#
#     # May expect a different frame_size??
#     return Processor(in_processor, out_processor)
#endregion

# fps seems to be required by all online-supporting processors if they are set as online=True
def _create_args(fps: int, **kwargs) -> dict[str, Any]:
    return { "online": True, "fps": fps, **kwargs }

def _create_rnn_tempo_args(fps: int, **kwargs) -> dict:
    return _create_args(
        # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TempoDetector#L119
        fps = fps,

        # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TempoDetector#L103
        method='comb',
        min_bpm=40.0,
        max_bpm=250.0,
        act_smooth=0.14,
        hist_smooth=9,
        hist_buffer=10.0,
        alpha=0.79,

        **kwargs
    )

def _create_rnn_beat_args(fps: int, **kwargs) -> dict:
    return _create_args(
        # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/DBNBeatTracker#L79
        fps = fps,

        # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/DBNBeatTracker#L73
        min_bpm = 55.0,
        max_bpm = 215.0,
        transition_lambda = 100,
        observation_lambda = 16,
        threshold = 0,
        correct = True,

        **kwargs
    )

def process_trackers(fps: int, hop_index: int, hop_size: int, frame_size: int, buffer: bytes, tracker: Processor, reset: bool = False, sample_rate: int = 44100, bits_per_sample: int = 32, num_channels: int = 1) -> list[Buffer]:
    if not isinstance(tracker, Processor):
        raise ValueError("Tracker must be a valid processor instance.")

    if bits_per_sample != 32 or sample_rate != 44100 or num_channels != 1:
        raise ValueError("Expected float32 mono input at 44,1 kHz")

    kwargs: dict[str, Any] = {
        "dtype": numpy.float32,
        "sample_rate": sample_rate,
        "frame_size": frame_size,
        "num_channels": num_channels,
        "fps": fps,
        "hop_size": hop_size,

        "reset": reset
    }

    start: Final[float] = (hop_index * hop_size) / sample_rate # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/madmom/audio/signal.py#L1479
    databuf: Final = numpy.frombuffer(buffer, dtype=numpy.float32)

    sig: Final = madmom.audio.Signal(databuf, start=start, **kwargs)
    mid: Final = tracker.in_processor.process(sig, **kwargs)

    assert len(tracker.out_processors) == len(tracker.results)
    for i in range(len(tracker.out_processors)):
        res = tracker.out_processors[i].process(mid, **kwargs)
        assert isinstance(res, numpy.ndarray)
        tracker.results[i] = res

    return tracker.results # type: ignore
