# Python >=3.13

# pyright: basic

from collections.abc import Buffer
import madmom # pyright: ignore [reportMissingTypeStubs]
from madmom.processors import IOProcessor
import numpy


def create_beat_tracker(fps: int) -> IOProcessor:
    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/DBNBeatTracker#L79
    args = _create_args(fps = fps)

    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/DBNBeatTracker#L86
    in_processor = madmom.features.RNNBeatProcessor(**args)
    out_processor = madmom.features.DBNBeatTrackingProcessor(**args)

    return IOProcessor(in_processor, out_processor)

def create_tcn_tempo_tracker(fps: int) -> IOProcessor:
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

    return IOProcessor(in_processor, out_processor)

def create_legacy_tempo_tracker(fps: int) -> IOProcessor:
    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TempoDetector#L119
    args = _create_args(fps = fps)

    # https://github.com/CPJKU/madmom/blob/27f032e8947204902c675e5e341a3faf5dc86dae/bin/TempoDetector#L126
    in_processor = madmom.features.RNNBeatProcessor(**args)
    out_processor = madmom.features.TempoEstimationProcessor(**args)

    return IOProcessor(in_processor, out_processor)

def create_onset_tracker(fps: int) -> IOProcessor:
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

    return IOProcessor(in_processor, out_processor)

# fps seems to be required by all online-supporting processors if they are set as online=True
def _create_args(fps: int, **kwargs) -> dict:
    return { "online": True, "fps": fps, **kwargs }

def process_tracker(tracker: IOProcessor, data: bytes, sample_rate: int) -> Buffer:
    assert isinstance(tracker, IOProcessor)
    databuf = numpy.frombuffer(data, dtype=numpy.uint8)
    signal = madmom.audio.Signal(databuf, sample_rate=sample_rate)
    return tracker.process(data=signal, reset=False)

def _main():
    print("Started testing of madmom-C# interop implementation.")


if __name__ == "__main__":
    _main()
