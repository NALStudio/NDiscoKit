# Python >=3.13

# pyright: basic

from collections.abc import Buffer
from typing import Any, Final, NamedTuple
import madmom # pyright: ignore [reportMissingTypeStubs]
from madmom.processors import BufferProcessor
import numpy

class Processor(NamedTuple):
    in_processor: madmom.processors.Processor
    out_processor: madmom.processors.Processor

def create_beat_tracker(fps: int) -> Processor:
    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/DBNBeatTracker#L79
    args = _create_args(fps = fps)

    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/DBNBeatTracker#L86
    in_processor = madmom.features.RNNBeatProcessor(**args)
    out_processor = madmom.features.DBNBeatTrackingProcessor(**args)

    return Processor(in_processor, out_processor)

def create_tcn_tempo_tracker(fps: int) -> Processor:
    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TCNTempoDetector#L65
    args = _create_args(
        fps=fps,
        tasks = (1, ),
        interpolate = True,
        method = None,
        act_smooth = None
    )

    args["histogram_processor"] = madmom.features.tempo.TCNTempoHistogramProcessor(**args)

    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TCNTempoDetector#L75
    in_processor = madmom.features.beats.TCNBeatProcessor(**args)
    out_processor = madmom.features.TempoEstimationProcessor(**args)

    return Processor(in_processor, out_processor)

def create_tempo_tracker(fps: int) -> Processor:
    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TempoDetector#L119
    args = _create_args(fps = fps)

    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TempoDetector#L126
    in_processor = madmom.features.RNNBeatProcessor(**args)
    out_processor = madmom.features.TempoEstimationProcessor(**args)

    return Processor(in_processor, out_processor)

def create_onset_tracker(fps: int) -> Processor:
    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/OnsetDetectorLL#L86
    args = _create_args(
        fps = fps,
        pre_max = 1. / fps,
        post_max = 0,
        post_avg = 0,
        online = True
    )

    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/OnsetDetectorLL#L97
    in_processor = madmom.features.RNNOnsetProcessor(**args)
    out_processor = madmom.features.OnsetPeakPickingProcessor(**args)

    # May expect a different frame_size?? 
    return Processor(in_processor, out_processor)

# fps seems to be required by all online-supporting processors if they are set as online=True
def _create_args(fps: int, **kwargs) -> dict:
    return { "online": True, "fps": fps, **kwargs }

def process_tracker(fps: int, hop_index: int, hop_size: int, frame_size: int, buffer: bytes, sample_rate: int, bits_per_sample: int, num_channels: int, tracker: Processor) -> Buffer:
    if bits_per_sample != 32 or sample_rate != 44100 or num_channels != 1:
        raise ValueError("Expected float32 mono input at 44,1 kHz")

    kwargs: dict[str, Any] = {
        "dtype": numpy.float32,
        "sample_rate": sample_rate,
        "frame_size": frame_size,
        "num_channels": num_channels,
        "fps": fps,
        "hop_size": hop_size,

        "reset": False
    }

    start: Final[float] = (hop_index * hop_size) / sample_rate # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/madmom/audio/signal.py#L1479
    databuf: Final = numpy.frombuffer(buffer, dtype=numpy.float32)

    sig: Final = madmom.audio.Signal(databuf, start=start, **kwargs)
    mid: Final = tracker.in_processor.process(sig, **kwargs)
    res: Final = tracker.out_processor.process(mid, **kwargs)
    
    assert(isinstance(res, numpy.ndarray));
    return res