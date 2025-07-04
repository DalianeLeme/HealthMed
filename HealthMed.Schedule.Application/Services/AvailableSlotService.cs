﻿using HealthMed.Schedule.Application.Interfaces;
using HealthMed.Schedule.Domain.Entities;
using HealthMed.Schedule.Domain.Interfaces;
using HealthMed.Shared.Events;
using HealthMed.Shared.Messaging;

namespace HealthMed.Schedule.Application.Services
{
    public class AvailableSlotService : IAvailableSlotService
    {
        private readonly IAvailableSlotRepository _repository;
        private readonly IEventPublisher _publisher;  

        public AvailableSlotService(
            IAvailableSlotRepository repository,
            IEventPublisher publisher)          
        {
            _repository = repository;
            _publisher = publisher;
        }
        public Task<List<AvailableSlot>> GetByDoctorAsync(Guid doctorId)
            => _repository.GetByDoctorAsync(doctorId);

        public async Task<bool> AddAsync(AvailableSlot slot)
        {
            var existing = await _repository.GetByDoctorAsync(slot.DoctorId);
            var conflict = existing.Any(s =>
                s.StartTime < slot.EndTime &&
                slot.StartTime < s.EndTime);

            if (conflict) return false;

            await _repository.AddAsync(slot);

            _publisher.Publish(
                nameof(SlotCreated),
                new SlotCreated(slot.Id, slot.DoctorId, slot.StartTime, slot.EndTime)
            );

            return true;
        }

        public async Task DeleteAsync(Guid id)
        {
            var slot = await _repository.GetByIdAsync(id);
            if (slot is null) return;

            await _repository.DeleteAsync(slot);

            var evt = new SlotDeleted(slot.Id, slot.DoctorId);

            _publisher.Publish(
                nameof(SlotDeleted),
                new SlotDeleted(slot.Id, slot.DoctorId)
            );
        }

        public async Task<bool> RemoveSlotByTimeAsync(Guid doctorId, DateTime startTime)
        {
            var slot = await _repository.GetByTimeAsync(doctorId, startTime);
            if (slot is null)
                return false;

            await DeleteAsync(slot.Id);
            return true;
        }

        public async Task<bool> UpdateAsync(AvailableSlot slot)
        {
            var existing = await _repository.GetByIdAsync(slot.Id);
            if (existing is null)
                return false;

            await _repository.UpdateAsync(slot);

            _publisher.Publish(
                nameof(SlotDeleted),
                new SlotDeleted(existing.Id, existing.DoctorId)
            );

            _publisher.Publish(
                nameof(SlotCreated),
                new SlotCreated(slot.Id, slot.DoctorId, slot.StartTime, slot.EndTime)
            );

            return true;
        }

        public Task<AvailableSlot?> GetByIdAsync(Guid id)
            => _repository.GetByIdAsync(id);
    }
}