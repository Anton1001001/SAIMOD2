namespace SAIMOD2;
public class Simulation
{
    private List<Event?> _events = new();
    private float _modelTime;
    private const float TimeEndOfSimulation = 20000f; // Time end of simulation

    private int _countType1; // Counter for type 1 parts
    private int _totalPartsType1;
    private int _countType2; // Counter for type 2 parts
    private int _totalPartsType2;
    private bool _robotFree = true; // Robot is free
    private bool _machine1Free = true; // Machine 1 is free
    private bool _machine2Free = true; // Machine 2 is free
    private int _completedBatches; // Number of completed batches
    private int _totalCompletedBatches;
    private float _totalProcessingTime; // Total processing time for parts
    private int _totalPartsProcessed; // Total number of parts processed
    private const int M1 = 10; // Batch size for type 1
    private const int M2 = 3; // Batch size for type 2

    private const float MeanTransportTimeToMachine = 5f; // Transport time to machine
    private const float VarianceTransportTimeToMachine = 1f;

    private const float MeanTransportTimeToExit = 10f; // Transport time to exit conveyor
    private const float VarianceTransportTimeToExit = 2f;
    
    private const float MeanProcessing = 15f;
    private const float VarianceProcessing = 5f;

    private const float MeanExponential = 20f;
    
    private const float DetailProbability = 0.7f;

    public static List<float> AvgMachine1List = new();

    private float _dt;
    private float _avgDetailsMachine1;
    private float _avgDetailsMachine2;
    
    Event? _currentEvent;

    public void Run()
    {
        
        var time = RandomGenerator.GetExponentialRandom(MeanExponential);
        
        ScheduleEvent(time, EventType.DetailArrival); // Schedule first part arrival

        while (_events.Any() && _modelTime < TimeEndOfSimulation)
        {
            _currentEvent = _events.First();
            _modelTime = _currentEvent!.Time;
            _events.RemoveAt(0);

            switch (_currentEvent.Type)
            {
                case EventType.DetailArrival:
                    HandleDetailArrival();
                    break;
                case EventType.CompleteBatchType1:
                    HandleCompleteBatch(1);
                    break;
                case EventType.CompleteBatchType2:
                    HandleCompleteBatch(2);
                    break;
                case EventType.StartTransportToMachine1:
                    HandleStartTransportToMachine(1);
                    break;
                case EventType.StartTransportToMachine2:
                    HandleStartTransportToMachine(2);
                    break;
                case EventType.FinishTransportToMachine1:
                    HandleFinishTransportToMachine(1);
                    break;
                case EventType.FinishTransportToMachine2:
                    HandleFinishTransportToMachine(2);
                    break;
                case EventType.FinishProcessingMachine1:
                    HandleFinishProcessing(1);
                    break;
                case EventType.FinishProcessingMachine2:
                    HandleFinishProcessing(2);
                    break;
                case EventType.TransportToExit:
                    HandleTransportToExit();
                    break;
                case EventType.TransportToExitEnd:
                    HandleTransportToExitEnd();
                    break;
            }
            LogEvent(_currentEvent); // Log the current event
        }
        
        AvgMachine1List.Add(_avgDetailsMachine1/ TimeEndOfSimulation);

        Console.WriteLine("Simulation finished.");
        Console.WriteLine($"Total parts processed: {_totalPartsProcessed}");
        Console.WriteLine($"Total batches processed: {_totalCompletedBatches}");
        Console.WriteLine($"Total parts type 1 processed: {_totalPartsType1} ({_totalPartsType1 / (float)_totalPartsProcessed * 100}%)");
        Console.WriteLine($"Total parts type 2 processed: {_totalPartsType2} ({_totalPartsType2 / (float)_totalPartsProcessed * 100}%)");
        Console.WriteLine($"Average batch processing time: {(_totalCompletedBatches > 0 ? _totalProcessingTime / _totalCompletedBatches : 0)} seconds");
        Console.WriteLine($"Average part processing time: {(_totalPartsProcessed > 0 ? _totalProcessingTime / _totalPartsProcessed : 0)} seconds");
        Console.WriteLine($"Avg details on machine1 {_avgDetailsMachine1 / TimeEndOfSimulation}");
        Console.WriteLine($"Avg details on machine2 {_avgDetailsMachine2 / TimeEndOfSimulation}");
    }

    private void LogEvent(Event? currentEvent)
    {
        Console.WriteLine($"[{_modelTime:F2}] Event: {currentEvent!.Type}");
        Console.WriteLine($"   Robot free: {_robotFree}");
        Console.WriteLine($"   Machine 1 free: {_machine1Free}");
        Console.WriteLine($"   Machine 2 free: {_machine2Free}");
        Console.WriteLine($"   Type 1 count: {_countType1}");
        Console.WriteLine($"   Type 2 count: {_countType2}");
        Console.WriteLine($"   Completed batches: {_completedBatches}");

        
        Console.WriteLine($"   Total parts processed: {_totalPartsProcessed}");
        Console.WriteLine($"   Total processing time: {_totalProcessingTime:F2}");
    }

    private void HandleDetailArrival()
    {
        int detailType = RandomGenerator.GenerateDetailType(DetailProbability);

        if (detailType == 1)
        {
            _countType1++;
            Console.WriteLine($"[{_modelTime:F2}] Type 1 detail arrived. Total count: {_countType1}");
            if (_countType1 >= M1 && _robotFree && _machine1Free)
            {
                ScheduleEvent(_modelTime, EventType.CompleteBatchType1);
                _countType1 -= M1;
                Console.WriteLine($"[{_modelTime:F2}] Batch of Type 1 is ready for processing.");
            }
        }
        else
        {
            _countType2++;
            Console.WriteLine($"[{_modelTime:F2}] Type 2 detail arrived. Total count: {_countType2}");
            if (_countType2 >= M2 && _robotFree && _machine2Free)
            {
                ScheduleEvent(_modelTime, EventType.CompleteBatchType2);
                _countType2 -= M2;
               Console.WriteLine($"[{_modelTime:F2}] Batch of Type 2 is ready for processing.");
            }
        }

        
        _dt = RandomGenerator.GetExponentialRandom(MeanExponential);

        _avgDetailsMachine1 += _countType1 * _dt;
        _avgDetailsMachine2 += _countType2 * _dt;
        
        
        // Schedule next detail arrival
        ScheduleEvent(_modelTime + _dt, EventType.DetailArrival);
    }

    private void HandleCompleteBatch(int type)
    {
        _robotFree = false;

        if (type == 1)
        {
            ScheduleEvent(_modelTime, EventType.StartTransportToMachine1);
            Console.WriteLine($"[{_modelTime:F2}] Starting transport for Batch Type 1.");
        }
        else
        {
            ScheduleEvent(_modelTime, EventType.StartTransportToMachine2);
            Console.WriteLine($"[{_modelTime:F2}] Starting transport for Batch Type 2.");
        }
    }

    private void HandleStartTransportToMachine(int machineType)
    {
        // Start transport to machine
        if (machineType == 1)
        {
            ScheduleEvent(_modelTime + RandomGenerator.GetNormalRandom(MeanTransportTimeToMachine, VarianceTransportTimeToMachine), EventType.FinishTransportToMachine1);
            Console.WriteLine($"[{_modelTime:F2}] Transporting to Machine 1.");
        }
        else
        {
            ScheduleEvent(_modelTime + RandomGenerator.GetNormalRandom(MeanTransportTimeToMachine, VarianceTransportTimeToMachine), EventType.FinishTransportToMachine2);
            Console.WriteLine($"[{_modelTime:F2}] Transporting to Machine 2.");
        }
    }

    private void HandleFinishTransportToMachine(int machineType)
    {
        // Finish transport and start processing
        if (machineType == 1)
        {
            _machine1Free = false;
            var time = RandomGenerator.GetNormalRandom(MeanProcessing, VarianceProcessing);
            _totalProcessingTime += time;
            
            ScheduleEvent(_modelTime + time, EventType.FinishProcessingMachine1);
            Console.WriteLine($"[{_modelTime:F2}] Finished transporting to Machine 1. Starting processing.");
        }
        else
        {
            _machine2Free = false;
            var time = RandomGenerator.GetNormalRandom(MeanProcessing, VarianceProcessing);
            _totalProcessingTime += time;
            ScheduleEvent(_modelTime + time, EventType.FinishProcessingMachine2);
            Console.WriteLine($"[{_modelTime:F2}] Finished transporting to Machine 2. Starting processing.");
        }
        _robotFree = true;
    }

    private void HandleFinishProcessing(int machineType)
    {
        _completedBatches++;
        _totalCompletedBatches++;
        _totalPartsProcessed += machineType == 1 ? M1 : M2;
        _totalPartsType1 += machineType == 1 ? M1 : 0;
        _totalPartsType2 += machineType == 2 ? M2 : 0;

        if (machineType == 1)
        {
            _machine1Free = true;
            Console.WriteLine($"[{_modelTime:F2}] Finished processing Batch Type 1.");
        }
        else
        {
            _machine2Free = true;
            Console.WriteLine($"[{_modelTime:F2}] Finished processing Batch Type 2.");
        }

        if (_completedBatches >= 3)
        {
            _completedBatches = 0;
            ScheduleEvent(_modelTime, EventType.TransportToExit);
            Console.WriteLine($"[{_modelTime:F2}] Three batches completed. Starting transport to exit.");
        }
        else
        {
            _robotFree = true; // Robot is free if no transport is needed
        }
    }
    private void HandleTransportToExit()
    {
        _robotFree = false; // Robot is busy transporting to exit conveyor
        ScheduleEvent(_modelTime + RandomGenerator.GetNormalRandom(MeanTransportTimeToExit, VarianceTransportTimeToExit), EventType.TransportToExitEnd);
        Console.WriteLine($"[{_modelTime:F2}] Transporting to exit conveyor.");
    }

    private void HandleTransportToExitEnd()
    {
        _robotFree = true; // Robot returns and becomes available
        Console.WriteLine($"[{_modelTime:F2}] Robot returned to the assembly station after delivering to the exit conveyor.");
    }

    private void ScheduleEvent(float time, EventType type)
    {
        _events.Add(new Event(time, type));
        _events = _events.OrderBy(e => e.Time).ToList();
    }

}
