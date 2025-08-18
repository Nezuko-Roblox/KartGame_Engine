using System;
using System.IO;
using UnityEngine;

public struct PhysicSpec
{
	public void Initialize()
	{
		this.wheelTranslate = new Vector3[4];
		this.wheelWidth = new float[4];
		this.itemSlotCapacity = 2U;
		this.useTransformBooster = false;
		this.mass = 100f;
		this.airFriction = 3f;
		this.dragFactor = 0.74f;
		this.forwardAccel = 2000f;
		this.backwardAccel = 1500f;
		this.gripBrake = 1800f;
		this.slipBrake = 1200f;
		this.maxSteerDeg = 8f;
		this.steerConstraint = 25f;
		this.frontGripFactor = 5f;
		this.rearGripFactor = 5f;
		this.driftTrigFactor = 0.2f;
		this.driftTrigTime = 0.2f;
		this.driftSlipFactor = 0.2f;
		this.driftEscapeForce = 2500f;
		this.cornerDrawFactor = 0.2f;
		this.driftLeanFactor = 0.07f;
		this.steerLeanFactor = 0.01f;
		this.driftMaxGauge = 4000f;
		this.normalBoosterTime = 3000f;
		this.teamBoosterTime = 4500f;
		this.animalBoosterTime = 4000f;
	}

	public void Load(string levelParam, string bodyParam)
	{
		if (levelParam == string.Empty)
		{
			return;
		}
		XMLElement xmlelement = new XMLElement();
		StringReader stringReader = new StringReader(levelParam);
		xmlelement.parseFromReader(stringReader);
		this.itemSlotCapacity = (uint)xmlelement.getIntAttribute("ItemSlotCapacity", 2);
		this.useTransformBooster = xmlelement.getBooleanAttribute("UseTransformBooster", "true", "false", false);
		this.mass = (float)xmlelement.getDoubleAttribute("Mass", 100.0);
		this.airFriction = (float)xmlelement.getDoubleAttribute("AirFriction", 3.0);
		this.dragFactor = (float)xmlelement.getDoubleAttribute("DragFactor", 0.74000000953674316);
		this.forwardAccel = (float)xmlelement.getDoubleAttribute("ForwardAccelForce", 2000.0);
		this.backwardAccel = (float)xmlelement.getDoubleAttribute("BackwardAccelForce", 1500.0);
		this.gripBrake = (float)xmlelement.getDoubleAttribute("GripBrakeForce", 1800.0);
		this.slipBrake = (float)xmlelement.getDoubleAttribute("SlipBrakeForce", 1200.0);
		this.maxSteerDeg = (float)xmlelement.getDoubleAttribute("MaxSteerAngle", 8.0);
		this.steerConstraint = (float)xmlelement.getDoubleAttribute("SteerConstraint", 25.0);
		this.frontGripFactor = (float)xmlelement.getDoubleAttribute("FrontGripFactor", 5.0);
		this.rearGripFactor = (float)xmlelement.getDoubleAttribute("RearGripFactor", 5.0);
		this.driftTrigFactor = (float)xmlelement.getDoubleAttribute("DriftTriggerFactor", 0.20000000298023224);
		this.driftTrigTime = (float)xmlelement.getDoubleAttribute("DriftTriggerTime", 0.20000000298023224);
		this.driftSlipFactor = (float)xmlelement.getDoubleAttribute("DriftSlipFactor", 0.20000000298023224);
		this.driftEscapeForce = (float)xmlelement.getDoubleAttribute("DriftEscapeForce", 2500.0);
		this.cornerDrawFactor = (float)xmlelement.getDoubleAttribute("CornerDrawFactor", 0.20000000298023224);
		this.driftLeanFactor = (float)xmlelement.getDoubleAttribute("DriftLeanFactor", 0.070000000298023224);
		this.steerLeanFactor = (float)xmlelement.getDoubleAttribute("SteerLeanFactor", 0.0099999997764825821);
		this.driftMaxGauge = (float)xmlelement.getDoubleAttribute("DriftMaxGauge", 4000.0);
		this.normalBoosterTime = (float)xmlelement.getDoubleAttribute("NormalBoosterTime", 3000.0);
		this.teamBoosterTime = (float)xmlelement.getDoubleAttribute("TeamBoosterTime", 4500.0);
		this.animalBoosterTime = (float)xmlelement.getDoubleAttribute("AnimalBoosterTime", 4000.0);
		if (bodyParam != string.Empty)
		{
			XMLElement xmlelement2 = new XMLElement();
			StringReader stringReader2 = new StringReader(bodyParam);
			xmlelement2.parseFromReader(stringReader2);
			this.itemSlotCapacity = (uint)xmlelement2.getIntAttribute("ItemSlotCapacity", 2);
			this.useTransformBooster = xmlelement2.getBooleanAttribute("UseTransformBooster", "true", "false", false);
			float num = this.mass;
			this.mass += (float)xmlelement2.getDoubleAttribute("Mass");
			this.airFriction += (float)xmlelement2.getDoubleAttribute("AirFriction");
			this.dragFactor += (float)xmlelement2.getDoubleAttribute("DragFactor");
			this.forwardAccel += (float)xmlelement2.getDoubleAttribute("ForwardAccelForce");
			this.backwardAccel += (float)xmlelement2.getDoubleAttribute("BackwardAccelForce");
			this.gripBrake += (float)xmlelement2.getDoubleAttribute("GripBrakeForce");
			this.slipBrake += (float)xmlelement2.getDoubleAttribute("SlipBrakeForce");
			this.maxSteerDeg += (float)xmlelement2.getDoubleAttribute("MaxSteerAngle");
			this.steerConstraint += (float)xmlelement2.getDoubleAttribute("SteerConstraint");
			this.frontGripFactor += (float)xmlelement2.getDoubleAttribute("FrontGripFactor");
			this.rearGripFactor += (float)xmlelement2.getDoubleAttribute("RearGripFactor");
			this.driftTrigFactor += (float)xmlelement2.getDoubleAttribute("DriftTriggerFactor");
			this.driftTrigTime += (float)xmlelement2.getDoubleAttribute("DriftTriggerTime");
			this.driftSlipFactor += (float)xmlelement2.getDoubleAttribute("DriftSlipFactor");
			this.driftEscapeForce += (float)xmlelement2.getDoubleAttribute("DriftEscapeForce");
			this.cornerDrawFactor += (float)xmlelement2.getDoubleAttribute("CornerDrawFactor");
			this.driftLeanFactor += (float)xmlelement2.getDoubleAttribute("DriftLeanFactor");
			this.steerLeanFactor += (float)xmlelement2.getDoubleAttribute("SteerLeanFactor");
			this.driftMaxGauge += (float)xmlelement2.getDoubleAttribute("DriftMaxGauge");
			this.normalBoosterTime += (float)xmlelement2.getDoubleAttribute("NormalBoosterTime");
			this.teamBoosterTime += (float)xmlelement2.getDoubleAttribute("TeamBoosterTime");
			this.animalBoosterTime += (float)xmlelement2.getDoubleAttribute("AnimalBoosterTime");
			float num2 = this.forwardAccel;
			this.forwardAccel = this.forwardAccel * this.mass / num;
			this.backwardAccel = this.backwardAccel * this.mass / num;
			this.gripBrake = this.gripBrake * this.mass / num;
			this.slipBrake = this.slipBrake * this.mass / num;
			this.driftEscapeForce = this.driftEscapeForce * this.mass / num;
			float num3 = (-this.airFriction + Mathf.Sqrt(this.airFriction * this.airFriction + 4f * this.dragFactor * num2)) * 0.5f / this.dragFactor;
			this.dragFactor = (this.forwardAccel - this.airFriction * num3) / num3 / num3;
		}
	}

	public float getMaxSteerRad()
	{
		return 3.14159274f * this.maxSteerDeg / 180f;
	}

	public override string ToString()
	{
		string text = "KartSpec\n";
		text = text + this.itemSlotCapacity.ToString() + "\n";
		text = text + this.useTransformBooster.ToString() + "\n";
		text = text + this.mass.ToString() + "\n";
		text = text + this.airFriction.ToString() + "\n";
		text = text + this.dragFactor.ToString() + "\n";
		text = text + this.forwardAccel.ToString() + "\n";
		text = text + this.backwardAccel.ToString() + "\n";
		text = text + this.gripBrake.ToString() + "\n";
		text = text + this.slipBrake.ToString() + "\n";
		text = text + this.maxSteerDeg.ToString() + "\n";
		text = text + this.steerConstraint.ToString() + "\n";
		text = text + this.frontGripFactor.ToString() + "\n";
		text = text + this.rearGripFactor.ToString() + "\n";
		text = text + this.driftTrigFactor.ToString() + "\n";
		text = text + this.driftTrigTime.ToString() + "\n";
		text = text + this.driftSlipFactor.ToString() + "\n";
		text = text + this.driftEscapeForce.ToString() + "\n";
		text = text + this.cornerDrawFactor.ToString() + "\n";
		text = text + this.driftLeanFactor.ToString() + "\n";
		text = text + this.steerLeanFactor.ToString() + "\n";
		text = text + this.driftMaxGauge.ToString() + "\n";
		text = text + this.normalBoosterTime.ToString() + "\n";
		text = text + this.teamBoosterTime.ToString() + "\n";
		return text + this.animalBoosterTime.ToString() + "\n";
	}

	public float airFriction;

	public float dragFactor;

	public float forwardAccel;

	public float backwardAccel;

	public float gripBrake;

	public float slipBrake;

	public float maxSteerDeg;

	public float steerConstraint;

	public float frontGripFactor;

	public float rearGripFactor;

	public float driftTrigFactor;

	public float driftTrigTime;

	public float driftSlipFactor;

	public float driftEscapeForce;

	public float cornerDrawFactor;

	public float driftLeanFactor;

	public float steerLeanFactor;

	public float driftMaxGauge;

	public float mass;

	public float width;

	public float length;

	public float springK;

	public float damperCopC;

	public float damperRebC;

	public Vector3[] wheelTranslate;

	public float[] wheelWidth;

	public float normalBoosterTime;

	public float teamBoosterTime;

	public float animalBoosterTime;

	public uint itemSlotCapacity;

	public bool useTransformBooster;
}
